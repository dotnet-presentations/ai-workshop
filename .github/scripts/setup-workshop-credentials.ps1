#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Collects the credentials needed to test the .NET AI Workshop end to end.

.DESCRIPTION
    Prompts for and stores the following User-scoped environment variables:

      WORKSHOP_AZURE_OPENAI_ENDPOINT      Microsoft Foundry (Azure OpenAI) endpoint
      WORKSHOP_AZURE_OPENAI_KEY           API key for that endpoint
      WORKSHOP_AZURE_OPENAI_CHAT          Chat deployment name       (samples expect gpt-5-mini)
      WORKSHOP_AZURE_OPENAI_EMBEDDING     Embedding deployment name  (samples expect text-embedding-3-small)
      WORKSHOP_AZURE_SUBSCRIPTION_ID      Subscription for the Part 11 azd deployment
      WORKSHOP_AZURE_LOCATION             Region for the Part 11 azd deployment
      WORKSHOP_LOCAL_MODEL_ENDPOINT       Optional. OpenAI-compatible local endpoint for Part 9
      WORKSHOP_LOCAL_MODEL_NAME           Optional. Local model name for Part 9

    The workshop projects themselves do not read these variables - the console
    samples read user secrets and the Aspire app reads a connection string. Use
    -ApplyUserSecrets to push the collected values into those projects.

.PARAMETER Force
    Re-prompt for variables that are already set. Use this when a key has been
    rotated or an endpoint has changed.

.PARAMETER ApplyUserSecrets
    After collecting values, write them into the user secrets of the workshop
    projects (Parts 2, 3, 8, 9) and the Part 11 AppHost connection string.

.PARAMETER SkipOptional
    Do not prompt for the optional local model values.

.EXAMPLE
    ./setup-workshop-credentials.ps1

.EXAMPLE
    ./setup-workshop-credentials.ps1 -Force -ApplyUserSecrets

.NOTES
    Requires PowerShell 7 or later.

    API keys are stored in plaintext in User-scoped environment variables so they
    survive across terminal sessions. Do not run this on a shared machine. To
    remove them afterwards, run:

        [Environment]::SetEnvironmentVariable('WORKSHOP_AZURE_OPENAI_KEY', $null, 'User')
#>

[CmdletBinding()]
param(
    [switch]$Force,
    [switch]$ApplyUserSecrets,
    [switch]$SkipOptional
)

$ErrorActionPreference = 'Stop'

# Deployment names hardcoded in the sample code. If the tester's resource uses
# different names, the samples need a source edit - warn rather than fail.
$ExpectedChatDeployment = 'gpt-5-mini'
$ExpectedEmbeddingDeployment = 'text-embedding-3-small'

function Request-WorkshopVariable {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Prompt,
        [string]$Help,
        [string]$Default,
        [switch]$Secret,
        [switch]$Optional
    )

    $current = [Environment]::GetEnvironmentVariable($Name, 'Process')
    if (-not $current) {
        $current = [Environment]::GetEnvironmentVariable($Name, 'User')
    }

    if ($current -and -not $Force) {
        Write-Host "  [set] $Name" -ForegroundColor Green
        return
    }

    if ($Help) { Write-Host "  $Help" -ForegroundColor DarkGray }

    if ($current) {
        $shown = if ($Secret) { '********' } else { $current }
        Write-Host "  current value: $shown" -ForegroundColor DarkGray
    }

    $hint = $Prompt
    if ($Default) { $hint += " [$Default]" }
    elseif ($current) { $hint += ' [Enter to keep current]' }
    elseif ($Optional) { $hint += ' [Enter to skip]' }

    if ($Secret) {
        $secure = Read-Host -Prompt "  $hint" -AsSecureString
        $value = [System.Net.NetworkCredential]::new('', $secure).Password
    }
    else {
        $value = (Read-Host -Prompt "  $hint").Trim()
    }

    if (-not $value) {
        if ($current) {
            Write-Host "  [kept] $Name unchanged" -ForegroundColor Green
            return
        }
        if ($Default) {
            $value = $Default
        }
        else {
            $colour = if ($Optional) { 'DarkGray' } else { 'Yellow' }
            Write-Host "  [skipped] $Name not set" -ForegroundColor $colour
            return
        }
    }

    [Environment]::SetEnvironmentVariable($Name, $value, 'Process')
    [Environment]::SetEnvironmentVariable($Name, $value, 'User')
    Write-Host "  [saved] $Name" -ForegroundColor Green
}

function Set-ProjectSecret {
    param(
        [Parameter(Mandatory)][string]$ProjectPath,
        [Parameter(Mandatory)][string]$Key,
        [Parameter(Mandatory)][string]$Value
    )

    dotnet user-secrets set $Key $Value --project $ProjectPath 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [failed] $Key in $(Split-Path $ProjectPath -Leaf)" -ForegroundColor Red
        return $false
    }
    return $true
}

function Write-WorkshopUserSecrets {
    $repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..' '..')

    if (-not $env:WORKSHOP_AZURE_OPENAI_ENDPOINT -or -not $env:WORKSHOP_AZURE_OPENAI_KEY) {
        Write-Host "Cannot apply user secrets: endpoint and key are both required." -ForegroundColor Red
        return
    }

    # Projects that read AzureOpenAI:Endpoint / AzureOpenAI:Key directly.
    # Parts 2, 3 and 8 are console samples; Part 9 is the eShopLite Aspire solution,
    # whose Products and Store projects each talk to Azure OpenAI.
    $consoleProjects = @(
        'Part 02 - Build Chat App/ChatApp'
        'Part 03 - Add RAG/RagChatApp'
        'Part 08 - Agent Framework Basics/AgentApp'
        'Part 09 - Adding AI to an Existing App/eShopLite/Products'
        'Part 09 - Adding AI to an Existing App/eShopLite/Store'
    )

    foreach ($relative in $consoleProjects) {
        $project = Join-Path $repoRoot $relative
        if (-not (Test-Path $project)) {
            Write-Host "  [missing] $relative" -ForegroundColor Yellow
            continue
        }

        dotnet user-secrets init --project $project 2>&1 | Out-Null

        $ok = Set-ProjectSecret $project 'AzureOpenAI:Endpoint' $env:WORKSHOP_AZURE_OPENAI_ENDPOINT
        $ok = (Set-ProjectSecret $project 'AzureOpenAI:Key' $env:WORKSHOP_AZURE_OPENAI_KEY) -and $ok

        # Part 9's optional observability assistant can run against a local model.
        if ($relative -like '*eShopLite/Store' -and $env:WORKSHOP_LOCAL_MODEL_ENDPOINT -and $env:WORKSHOP_LOCAL_MODEL_NAME) {
            $ok = (Set-ProjectSecret $project 'LocalModel:Endpoint' $env:WORKSHOP_LOCAL_MODEL_ENDPOINT) -and $ok
            $ok = (Set-ProjectSecret $project 'LocalModel:Model' $env:WORKSHOP_LOCAL_MODEL_NAME) -and $ok
        }

        if ($ok) { Write-Host "  [ok] $relative" -ForegroundColor Green }
    }

    # Part 11 / Part 4 Aspire app: a single composed connection string on the AppHost.
    $appHost = Join-Path $repoRoot 'Part 11 - Deployment/GenAiLab/GenAiLab.AppHost'
    if (Test-Path $appHost) {
        dotnet user-secrets init --project $appHost 2>&1 | Out-Null
        $connection = "Endpoint=$($env:WORKSHOP_AZURE_OPENAI_ENDPOINT);Key=$($env:WORKSHOP_AZURE_OPENAI_KEY)"
        if (Set-ProjectSecret $appHost 'ConnectionStrings:openai' $connection) {
            Write-Host "  [ok] Part 11 - Deployment/GenAiLab/GenAiLab.AppHost" -ForegroundColor Green
        }
    }
    else {
        Write-Host "  [missing] Part 11 - Deployment/GenAiLab/GenAiLab.AppHost" -ForegroundColor Yellow
    }
}

Write-Host ''
Write-Host '=== AI Workshop credential setup ===' -ForegroundColor Cyan
if ($Force) { Write-Host 'Force enabled: re-prompting for every value.' -ForegroundColor Yellow }
Write-Host ''

Write-Host 'Microsoft Foundry (Azure OpenAI) - required for Parts 2, 3, 4, 8, 9, 11' -ForegroundColor White
Request-WorkshopVariable -Name 'WORKSHOP_AZURE_OPENAI_ENDPOINT' `
    -Prompt 'Endpoint' `
    -Help 'Resource endpoint, for example https://your-resource.openai.azure.com/'
Request-WorkshopVariable -Name 'WORKSHOP_AZURE_OPENAI_KEY' `
    -Prompt 'API key' -Secret `
    -Help 'API key from the Azure portal. Stored in plaintext in a User environment variable.'
Request-WorkshopVariable -Name 'WORKSHOP_AZURE_OPENAI_CHAT' `
    -Prompt 'Chat deployment name' -Default $ExpectedChatDeployment `
    -Help 'The deployment must already exist on the resource.'
Request-WorkshopVariable -Name 'WORKSHOP_AZURE_OPENAI_EMBEDDING' `
    -Prompt 'Embedding deployment name' -Default $ExpectedEmbeddingDeployment `
    -Help 'Needed for the RAG exercises in Part 3 and the ingestion pipeline in Parts 4 and 11.'

Write-Host ''
Write-Host 'Azure deployment - required only for Part 11' -ForegroundColor White
Request-WorkshopVariable -Name 'WORKSHOP_AZURE_SUBSCRIPTION_ID' -Prompt 'Subscription ID' -Optional
Request-WorkshopVariable -Name 'WORKSHOP_AZURE_LOCATION' -Prompt 'Location' -Optional `
    -Help 'For example eastus, westus2, centralus, westeurope, eastasia.'

if (-not $SkipOptional) {
    Write-Host ''
    Write-Host 'Local model - optional, used by the operations module in Part 9 and by Part 10' -ForegroundColor White
    Request-WorkshopVariable -Name 'WORKSHOP_LOCAL_MODEL_ENDPOINT' -Prompt 'Local endpoint' -Optional `
        -Help 'Ollama: http://localhost:11434/v1   Foundry Local: http://localhost:5273/v1'
    Request-WorkshopVariable -Name 'WORKSHOP_LOCAL_MODEL_NAME' -Prompt 'Local model name' -Optional `
        -Help 'Ollama: llama3.2   Foundry Local: phi-4-mini'
}

Write-Host ''
Write-Host '=== Status ===' -ForegroundColor Cyan

$status = [ordered]@{
    'Foundry endpoint'      = $env:WORKSHOP_AZURE_OPENAI_ENDPOINT
    'Foundry key'           = $env:WORKSHOP_AZURE_OPENAI_KEY
    'Chat deployment'       = $env:WORKSHOP_AZURE_OPENAI_CHAT
    'Embedding deployment'  = $env:WORKSHOP_AZURE_OPENAI_EMBEDDING
    'Azure subscription'    = $env:WORKSHOP_AZURE_SUBSCRIPTION_ID
    'Azure location'        = $env:WORKSHOP_AZURE_LOCATION
    'Local model endpoint'  = $env:WORKSHOP_LOCAL_MODEL_ENDPOINT
    'Local model name'      = $env:WORKSHOP_LOCAL_MODEL_NAME
}

foreach ($item in $status.GetEnumerator()) {
    $isSet = -not [string]::IsNullOrWhiteSpace($item.Value)
    $label = if ($isSet) { 'Set' } else { 'Not set' }
    $colour = if ($isSet) { 'Green' } else { 'DarkGray' }
    Write-Host ("  {0,-22} {1}" -f $item.Key, $label) -ForegroundColor $colour
}

# The deployment names are compiled into the samples, so a mismatch is a code change.
$mismatched = @()
if ($env:WORKSHOP_AZURE_OPENAI_CHAT -and $env:WORKSHOP_AZURE_OPENAI_CHAT -ne $ExpectedChatDeployment) {
    $mismatched += "chat ('$env:WORKSHOP_AZURE_OPENAI_CHAT' instead of '$ExpectedChatDeployment')"
}
if ($env:WORKSHOP_AZURE_OPENAI_EMBEDDING -and $env:WORKSHOP_AZURE_OPENAI_EMBEDDING -ne $ExpectedEmbeddingDeployment) {
    $mismatched += "embedding ('$env:WORKSHOP_AZURE_OPENAI_EMBEDDING' instead of '$ExpectedEmbeddingDeployment')"
}

if ($mismatched.Count -gt 0) {
    Write-Host ''
    Write-Host 'Warning: deployment names differ from the ones the samples hardcode:' -ForegroundColor Yellow
    $mismatched | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
    Write-Host '  Update the model constants in these files before running the samples:' -ForegroundColor Yellow
    Write-Host '    Part 02 - Build Chat App/ChatApp/Program.cs' -ForegroundColor DarkGray
    Write-Host '    Part 03 - Add RAG/RagChatApp/Program.cs (and checkpoints/*.cs)' -ForegroundColor DarkGray
    Write-Host '    Part 08 - Agent Framework Basics/AgentApp/Program.cs' -ForegroundColor DarkGray
        Write-Host '    Part 09 - Adding AI to an Existing App/eShopLite/Products/Program.cs' -ForegroundColor DarkGray
        Write-Host '    Part 09 - Adding AI to an Existing App/eShopLite/Store/Program.cs' -ForegroundColor DarkGray
    Write-Host '    Part 11 - Deployment/GenAiLab/GenAiLab.Web/Program.cs' -ForegroundColor DarkGray
}

if ($ApplyUserSecrets) {
    Write-Host ''
    Write-Host '=== Applying user secrets ===' -ForegroundColor Cyan
    Write-WorkshopUserSecrets
}
else {
    Write-Host ''
    Write-Host 'These variables are not read by the workshop projects themselves.' -ForegroundColor Yellow
    Write-Host 'Re-run with -ApplyUserSecrets to write them into Parts 2, 3, 8, 9 and the Part 11 AppHost.' -ForegroundColor Yellow
}

Write-Host ''
Write-Host 'New environment variables are not visible to already-running terminals or IDEs.' -ForegroundColor DarkGray
Write-Host 'Restart them if a project cannot see the values.' -ForegroundColor DarkGray
Write-Host ''
