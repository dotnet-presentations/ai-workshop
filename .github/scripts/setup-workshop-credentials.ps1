#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sets up environment variables for AI Workshop credentials.

.DESCRIPTION
    This script checks for and configures the required environment variables for the AI Workshop:
    - WORKSHOP_GITHUB_TOKEN: GitHub personal access token for GitHub Models
    - WORKSHOP_AZURE_OPENAI_ENDPOINT: Azure OpenAI service endpoint URL
    - WORKSHOP_AZURE_OPENAI_KEY: Azure OpenAI service API key
    - WORKSHOP_AZURE_SUBSCRIPTION_ID: Azure subscription ID for deployment
    - WORKSHOP_AZURE_LOCATION: Azure location/region for deployment
    - WORKSHOP_AZURE_SEARCH_ENDPOINT: Azure AI Search service endpoint (optional)
    - WORKSHOP_AZURE_SEARCH_KEY: Azure AI Search service API key (optional)
    
    If variables are not found, the script prompts for them and saves them to the user environment.

.EXAMPLE
    .\setup-workshop-credentials.ps1
    
.NOTES
    After running this script for the first time, restart your terminal/IDE to ensure 
    environment variables are loaded properly.
#>

function Set-WorkshopCredentials {
    Write-Host "=== AI Workshop Credential Setup ===" -ForegroundColor Cyan
    Write-Host ""
    
    # Check for GitHub Token
    if (-not $env:WORKSHOP_GITHUB_TOKEN) {
        Write-Host "GitHub token not found in environment variable WORKSHOP_GITHUB_TOKEN" -ForegroundColor Yellow
        Write-Host "Create a classic token (no scopes needed) or fine-grained token with 'models:read' scope"
        Write-Host "See: https://github.blog/changelog/2025-05-15-modelsread-now-required-for-github-models-access/"
        Write-Host ""
        
        $githubToken = Read-Host -Prompt "Enter your GitHub personal access token" -AsSecureString
        if ($githubToken.Length -gt 0) {
            $env:WORKSHOP_GITHUB_TOKEN = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($githubToken))
            [Environment]::SetEnvironmentVariable("WORKSHOP_GITHUB_TOKEN", $env:WORKSHOP_GITHUB_TOKEN, "User")
            Write-Host "✓ GitHub token saved to environment variable WORKSHOP_GITHUB_TOKEN" -ForegroundColor Green
        } else {
            Write-Host "⚠ GitHub token was empty, skipping..." -ForegroundColor Yellow
        }
    } else {
        Write-Host "✓ GitHub token found in environment variable WORKSHOP_GITHUB_TOKEN" -ForegroundColor Green
    }
    
    Write-Host ""
    
    # Check for Azure OpenAI Endpoint
    if (-not $env:WORKSHOP_AZURE_OPENAI_ENDPOINT) {
        Write-Host "Azure OpenAI endpoint not found in environment variable WORKSHOP_AZURE_OPENAI_ENDPOINT" -ForegroundColor Yellow
        Write-Host "This should be your Azure OpenAI resource endpoint URL"
        Write-Host ""
        
        $azureEndpoint = Read-Host -Prompt "Enter your Azure OpenAI endpoint (e.g., https://your-resource.openai.azure.com/)"
        if ($azureEndpoint.Length -gt 0) {
            $env:WORKSHOP_AZURE_OPENAI_ENDPOINT = $azureEndpoint
            [Environment]::SetEnvironmentVariable("WORKSHOP_AZURE_OPENAI_ENDPOINT", $azureEndpoint, "User")
            Write-Host "✓ Azure OpenAI endpoint saved to environment variable WORKSHOP_AZURE_OPENAI_ENDPOINT" -ForegroundColor Green
        } else {
            Write-Host "⚠ Azure OpenAI endpoint was empty, skipping..." -ForegroundColor Yellow
        }
    } else {
        Write-Host "✓ Azure OpenAI endpoint found in environment variable WORKSHOP_AZURE_OPENAI_ENDPOINT" -ForegroundColor Green
    }
    
    Write-Host ""
    
    # Check for Azure OpenAI Key
    if (-not $env:WORKSHOP_AZURE_OPENAI_KEY) {
        Write-Host "Azure OpenAI key not found in environment variable WORKSHOP_AZURE_OPENAI_KEY" -ForegroundColor Yellow
        Write-Host "This should be your Azure OpenAI API key from the Azure portal"
        Write-Host ""
        
        $azureKey = Read-Host -Prompt "Enter your Azure OpenAI API key" -AsSecureString
        if ($azureKey.Length -gt 0) {
            $env:WORKSHOP_AZURE_OPENAI_KEY = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($azureKey))
                [Environment]::SetEnvironmentVariable("WORKSHOP_AZURE_OPENAI_KEY", $AzureOpenAIKey, "User")
}

# Check for Azure Subscription ID
$AzureSubscriptionId = [Environment]::GetEnvironmentVariable("WORKSHOP_AZURE_SUBSCRIPTION_ID", "User")
if (-not $AzureSubscriptionId) {
    Write-Host "`n🔑 Azure Subscription ID not found. Please provide your Azure subscription ID for deployment." -ForegroundColor Yellow
    Write-Host "You can find this in the Azure Portal under Subscriptions." -ForegroundColor Gray
    
    do {
        $AzureSubscriptionId = Read-Host "Enter Azure Subscription ID"
    } while ([string]::IsNullOrWhiteSpace($AzureSubscriptionId))
    
    Write-Host "✅ Saving Azure Subscription ID to environment variables..." -ForegroundColor Green
    [Environment]::SetEnvironmentVariable("WORKSHOP_AZURE_SUBSCRIPTION_ID", $AzureSubscriptionId, "User")
}

# Check for Azure Location
$AzureLocation = [Environment]::GetEnvironmentVariable("WORKSHOP_AZURE_LOCATION", "User")
if (-not $AzureLocation) {
    Write-Host "`n🌍 Azure Location not found. Please provide your preferred Azure region for deployment." -ForegroundColor Yellow
    Write-Host "Common options: eastus, westus2, centralus, westeurope, eastasia" -ForegroundColor Gray
    
    do {
        $AzureLocation = Read-Host "Enter Azure Location (e.g., eastus)"
    } while ([string]::IsNullOrWhiteSpace($AzureLocation))
    
    Write-Host "✅ Saving Azure Location to environment variables..." -ForegroundColor Green
    [Environment]::SetEnvironmentVariable("WORKSHOP_AZURE_LOCATION", $AzureLocation, "User")
}
            Write-Host "✓ Azure OpenAI key saved to environment variable WORKSHOP_AZURE_OPENAI_KEY" -ForegroundColor Green
        } else {
            Write-Host "⚠ Azure OpenAI key was empty, skipping..." -ForegroundColor Yellow
        }
    } else {
        Write-Host "✓ Azure OpenAI key found in environment variable WORKSHOP_AZURE_OPENAI_KEY" -ForegroundColor Green
    }
    
    Write-Host ""
    
    # Check for Azure AI Search credentials (optional)
    Write-Host "=== Optional: Azure AI Search Credentials ===" -ForegroundColor Cyan
    Write-Host "Azure AI Search is an alternative to Qdrant for vector storage in Part 6 deployment." -ForegroundColor Yellow
    Write-Host "These credentials are optional and only needed if you want to use Azure AI Search instead of Qdrant." -ForegroundColor Yellow
    Write-Host ""
    
    $useAzureSearch = Read-Host "Do you want to configure Azure AI Search credentials? (y/N)"
    if ($useAzureSearch -eq 'y' -or $useAzureSearch -eq 'Y') {
        # Check for Azure AI Search Endpoint
        if (-not $env:WORKSHOP_AZURE_SEARCH_ENDPOINT) {
            Write-Host "Azure AI Search endpoint not found in environment variable WORKSHOP_AZURE_SEARCH_ENDPOINT" -ForegroundColor Yellow
            Write-Host "This should be your Azure AI Search service endpoint URL" -ForegroundColor Gray
            Write-Host ""
            
            $azureSearchEndpoint = Read-Host -Prompt "Enter your Azure AI Search endpoint (e.g., https://your-search-service.search.windows.net)"
            if ($azureSearchEndpoint.Length -gt 0) {
                $env:WORKSHOP_AZURE_SEARCH_ENDPOINT = $azureSearchEndpoint
                [Environment]::SetEnvironmentVariable("WORKSHOP_AZURE_SEARCH_ENDPOINT", $azureSearchEndpoint, "User")
                Write-Host "✓ Azure AI Search endpoint saved to environment variable WORKSHOP_AZURE_SEARCH_ENDPOINT" -ForegroundColor Green
            } else {
                Write-Host "⚠ Azure AI Search endpoint was empty, skipping..." -ForegroundColor Yellow
            }
        } else {
            Write-Host "✓ Azure AI Search endpoint found in environment variable WORKSHOP_AZURE_SEARCH_ENDPOINT" -ForegroundColor Green
        }
        
        Write-Host ""
        
        # Check for Azure AI Search Key
        if (-not $env:WORKSHOP_AZURE_SEARCH_KEY) {
            Write-Host "Azure AI Search key not found in environment variable WORKSHOP_AZURE_SEARCH_KEY" -ForegroundColor Yellow
            Write-Host "This should be your Azure AI Search admin key from the Azure portal" -ForegroundColor Gray
            Write-Host ""
            
            $azureSearchKey = Read-Host -Prompt "Enter your Azure AI Search admin key" -AsSecureString
            if ($azureSearchKey.Length -gt 0) {
                $env:WORKSHOP_AZURE_SEARCH_KEY = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($azureSearchKey))
                [Environment]::SetEnvironmentVariable("WORKSHOP_AZURE_SEARCH_KEY", $env:WORKSHOP_AZURE_SEARCH_KEY, "User")
                Write-Host "✓ Azure AI Search key saved to environment variable WORKSHOP_AZURE_SEARCH_KEY" -ForegroundColor Green
            } else {
                Write-Host "⚠ Azure AI Search key was empty, skipping..." -ForegroundColor Yellow
            }
        } else {
            Write-Host "✓ Azure AI Search key found in environment variable WORKSHOP_AZURE_SEARCH_KEY" -ForegroundColor Green
        }
    } else {
        Write-Host "⚠ Skipping Azure AI Search configuration. Using Qdrant for vector storage." -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "=== Credential Setup Complete ===" -ForegroundColor Cyan
    
    # Show current status
    Write-Host ""
    Write-Host "Current credential status:" -ForegroundColor White
    Write-Host "  GitHub Token: $($env:WORKSHOP_GITHUB_TOKEN -ne $null -and $env:WORKSHOP_GITHUB_TOKEN.Length -gt 0 ? 'Set' : 'Not Set')"
    Write-Host "  Azure Endpoint: $($env:WORKSHOP_AZURE_OPENAI_ENDPOINT -ne $null -and $env:WORKSHOP_AZURE_OPENAI_ENDPOINT.Length -gt 0 ? 'Set' : 'Not Set')"
    Write-Host "  Azure Key: $($env:WORKSHOP_AZURE_OPENAI_KEY -ne $null -and $env:WORKSHOP_AZURE_OPENAI_KEY.Length -gt 0 ? 'Set' : 'Not Set')"
    Write-Host "  Azure Subscription: $($env:WORKSHOP_AZURE_SUBSCRIPTION_ID -ne $null -and $env:WORKSHOP_AZURE_SUBSCRIPTION_ID.Length -gt 0 ? 'Set' : 'Not Set')"
    Write-Host "  Azure Location: $($env:WORKSHOP_AZURE_LOCATION -ne $null -and $env:WORKSHOP_AZURE_LOCATION.Length -gt 0 ? 'Set' : 'Not Set')"
    Write-Host "  Azure AI Search Endpoint: $($env:WORKSHOP_AZURE_SEARCH_ENDPOINT -ne $null -and $env:WORKSHOP_AZURE_SEARCH_ENDPOINT.Length -gt 0 ? 'Set' : 'Not Set')" -ForegroundColor Gray
    Write-Host "  Azure AI Search Key: $($env:WORKSHOP_AZURE_SEARCH_KEY -ne $null -and $env:WORKSHOP_AZURE_SEARCH_KEY.Length -gt 0 ? 'Set' : 'Not Set')" -ForegroundColor Gray
    
    if ($env:WORKSHOP_GITHUB_TOKEN -or $env:WORKSHOP_AZURE_OPENAI_ENDPOINT -or $env:WORKSHOP_AZURE_OPENAI_KEY) {
        Write-Host ""
        Write-Host "Note: If you set new environment variables, restart your terminal/IDE to ensure they are loaded." -ForegroundColor Yellow
        Write-Host "You can now proceed with the AI Workshop!" -ForegroundColor Green
    }
}

# Run the credential setup
Set-WorkshopCredentials
