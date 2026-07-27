# Part 7 (Optional / bonus): MCP Publishing and Distribution

> **⏱️ Estimated Time:** 30-45 minutes
>
> **Prerequisites**: Complete [Part 5: MCP Server Basics](../Part%2005%20-%20MCP%20Server%20Basics/README.md). [Part 6: Enhanced MCP Server](../Part%2006%20-%20Enhanced%20MCP%20Server/README.md) is recommended if you followed the optional advanced track.

## Overview

In this final part of the MCP workshop, you'll learn how to **package**, **publish**, and **distribute** your MCP servers so other developers and organizations can easily use them. This part covers the complete lifecycle from development to production deployment of MCP servers.

## Learning Objectives

By the end of this part, you will:

- ✅ Understand MCP packaging and distribution strategies
- ✅ Configure MCP servers for production deployment
- ✅ Publish MCP servers to NuGet for easy consumption
- ✅ Create professional documentation for MCP server consumers
- ✅ Implement versioning and update strategies
- ✅ Apply security best practices for published MCP servers

## Publishing Strategies Overview

There are several ways to distribute MCP servers:

### 1. **NuGet Package Distribution** (Recommended)

- ✅ Easy installation with `dnx` command
- ✅ Version management and dependency resolution
- ✅ Wide accessibility through NuGet.org
- ✅ Professional package management

### 2. **Source Code Distribution**

- ✅ Full transparency and customization
- ✅ Suitable for open-source projects
- ⚠️ Requires manual compilation by users
- ⚠️ More complex setup process

### 3. **Container Distribution**

- ✅ Consistent runtime environment
- ✅ Easy deployment to cloud platforms
- ⚠️ Larger distribution size
- ⚠️ Requires container knowledge

### 4. **Enterprise Internal Distribution**

- ✅ Control over internal tools and data
- ✅ Security through private repositories
- ✅ Custom authentication and authorization
- ⚠️ Limited to organization boundaries

## Step 1: Preparing for NuGet Publication

### Work on a copy

This part publishes the **MyMcpServer** you built in Part 5. Packing produces
seven `.nupkg` files and a `bin/` tree full of per-platform builds, so work on a
copy rather than editing the workshop snapshot in place:

```powershell
cd "Part 07 - MCP Publishing"
Copy-Item "..\Part 05 - MCP Server Basics\MyMcpServer" .\publish-lab\MyMcpServer -Recurse
Remove-Item .\publish-lab\MyMcpServer\bin, .\publish-lab\MyMcpServer\obj -Recurse -Force -ErrorAction SilentlyContinue
cd .\publish-lab\MyMcpServer
```

If you still have your own Part 5 project from when you ran
`dotnet new mcpserver`, use that instead.

### Configure package metadata

Open `MyMcpServer.csproj`. The `mcpserver` template already wrote the properties
that make this an MCP server, so **do not replace the file** — you are only
filling in publishing metadata.

Replace the `<!-- Set recommended package metadata -->` block with the following.
Leave everything above it exactly as it is:

```xml
    <!-- Set recommended package metadata -->
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageId>YourName.MyMcpServer</PackageId>
    <PackageVersion>1.0.0</PackageVersion>
    <Authors>Your Name</Authors>
    <Description>An MCP server that provides weather information tools for AI agents.</Description>
    <PackageTags>AI; MCP; server; stdio; weather</PackageTags>
    <PackageProjectUrl>https://github.com/your-username/weather-mcp-server</PackageProjectUrl>
    <RepositoryUrl>https://github.com/your-username/weather-mcp-server</RepositoryUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <ToolCommandName>weather-mcp-server</ToolCommandName>
```

> [!WARNING]
> Four things in the template's `.csproj` are what turn an ordinary console app
> into a publishable MCP server. Deleting any of them produces a package that
> builds and pushes but that no MCP client will recognise:
>
> | Setting | Why it has to stay |
> | --- | --- |
> | `<PackageType>McpServer</PackageType>` | Without it the package is only a `DotnetTool`, and MCP registries and clients skip it |
> | `<None Include=".mcp\server.json" Pack="true" PackagePath="/.mcp/" />` | Ships the server manifest inside the package |
> | `<RuntimeIdentifiers>` | Drives the per-platform packages MCP servers ship as |
> | `<SelfContained>`, `<PublishSelfContained>`, `<PublishSingleFile>` | Let the server run without a matching shared framework on the consumer's machine |
>
> `<ImplicitUsings>enable</ImplicitUsings>` also has to stay, or `WeatherTools.cs`
> stops compiling with `CS0246: The type or namespace name 'Task<>' could not be
> found`.

There is no `<McpServerName>` MSBuild property. The server's name comes from
`.mcp/server.json` and from the `WithStdioServerTransport` call in `Program.cs`.

### Configure MCP server metadata

`.mcp/server.json` is the manifest that registries read. The template ships it
with placeholders, so you are filling those in rather than rewriting the file.

> [!IMPORTANT]
> Replace every placeholder (`your-username`, `your repo name`, the package ID)
> with real values before pushing to any registry.

```json
{
  "$schema": "https://static.modelcontextprotocol.io/schemas/2025-10-17/server.schema.json",
  "description": "An MCP server that provides weather information tools for AI agents.",
  "name": "io.github.your-username/weather-mcp-server",
  "version": "1.0.0",
  "packages": [
    {
      "registryType": "nuget",
      "identifier": "YourName.MyMcpServer",
      "version": "1.0.0",
      "transport": {
        "type": "stdio"
      },
      "packageArguments": [],
      "environmentVariables": []
    }
  ],
  "repository": {
    "url": "https://github.com/your-username/weather-mcp-server",
    "source": "github"
  }
}
```

The `version` values here and `<PackageVersion>` in the `.csproj` have to match,
or the manifest describes a package that doesn't exist.

> [!NOTE]
> This schema is versioned and it moves. If your template scaffolded a different
> `$schema` URL, keep the one the template gave you and edit its fields instead
> of pasting this block wholesale.

## Step 2: Create Professional Documentation

### Write the package README

`<PackageReadmeFile>README.md</PackageReadmeFile>` means NuGet shows this
project's `README.md` on the package page, so that is the file to write. The
template put a short placeholder there. **Replace the contents** of
`README.md` with something a consumer can actually use:

````markdown
# MyMcpServer

A Model Context Protocol (MCP) server that provides weather information tools for AI agents like GitHub Copilot.

## Features

- 🌤️ **Current Weather**: Get real-time weather conditions for any city
- 📅 **Weather Forecast**: 5-day weather predictions with detailed information
- 🔧 **Easy Integration**: Works with VS Code, Visual Studio, and any MCP-compatible client
- ⚡ **Fast Response**: Optimized for quick weather data retrieval

## Installation

### Using dnx (Recommended)

```bash
dnx YourName.MyMcpServer --version 1.0.0 --yes
```

### Manual Configuration

Add to your `.vscode/mcp.json` or `.mcp.json`:

```json
{
  "servers": {
    "MyMcpServer": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "YourName.MyMcpServer",
        "--version",
        "1.0.0",
        "--yes"
      ]
    }
  }
}
```

## Usage Examples

### Get Current Weather

Ask your AI assistant:

```text
What's the current weather in Tokyo?
```

### Get Weather Forecast

Ask your AI assistant:

```text
Can you give me a 5-day forecast for London?
```

## Configuration

### Optional Environment Variables

- `WEATHER_API_KEY`: Enhance weather data accuracy (optional)
- `WEATHER_UNITS`: Set temperature units (celsius/fahrenheit, default: celsius)

### VS Code Configuration

```json
{
  "servers": {
    "MyMcpServer": {
      "type": "stdio", 
      "command": "dnx",
      "args": ["YourName.MyMcpServer", "--version", "1.0.0"],
      "env": {
        "WEATHER_UNITS": "fahrenheit"
      }
    }
  }
}
```

## Supported Tools

| Tool | Description | Parameters |
|------|-------------|------------|
| `GetCurrentWeather` | Current weather conditions | `city` (string) |
| `GetWeatherForecast` | 5-day weather forecast | `city` (string) |

## Requirements

- .NET 10.0 or higher
- MCP-compatible client (VS Code with Copilot, Visual Studio, etc.)

## License

MIT License - see LICENSE file for details.

## Support

- 🐛 [Report Issues](https://github.com/your-username/weather-mcp-server/issues)
- 📖 [Documentation](https://github.com/your-username/weather-mcp-server/wiki)
- 💬 [Discussions](https://github.com/your-username/weather-mcp-server/discussions)

````

## Step 3: Build and Test the Package

### Local Package Testing

1. **Build the package**:

   ```powershell
   dotnet pack -c Release
   ```

   This produces **seven** packages, not one:

   ```text
   YourName.MyMcpServer.1.0.0.nupkg
   YourName.MyMcpServer.win-x64.1.0.0.nupkg
   YourName.MyMcpServer.win-arm64.1.0.0.nupkg
   YourName.MyMcpServer.osx-arm64.1.0.0.nupkg
   YourName.MyMcpServer.linux-x64.1.0.0.nupkg
   YourName.MyMcpServer.linux-arm64.1.0.0.nupkg
   YourName.MyMcpServer.linux-musl-x64.1.0.0.nupkg
   ```

   That fan-out is what `<RuntimeIdentifiers>` and `<SelfContained>` buy you. The
   base package carries the metadata and the server manifest; each RID package
   carries a self-contained executable for one platform. A consumer's `dnx`
   resolves the right one for their machine, which is why an MCP server can run
   without the consumer installing a matching .NET runtime first.

2. **Install it as a local tool**:

   ```powershell
   dotnet tool install --global --add-source ./bin/Release YourName.MyMcpServer --version 1.0.0
   ```

   The output tells you the command name, which comes from `<ToolCommandName>`:

   ```text
   You can invoke the tool using the following command: weather-mcp-server
   ```

   > [!NOTE]
   > Don't expect `weather-mcp-server --help` to print anything. The template's
   > `Program.cs` doesn't parse arguments; it starts an stdio MCP server and
   > waits for a client on standard input. Run it bare and it will simply sit
   > there until you press Ctrl+C. The real test is pointing an MCP client at it,
   > which you do in Step 5.

3. **Uninstall after testing**:

   ```powershell
   dotnet tool uninstall --global YourName.MyMcpServer
   ```

### Validate Package Contents

```powershell
# Extract and examine package contents
Expand-Archive bin/Release/YourName.MyMcpServer.1.0.0.nupkg -DestinationPath temp
Get-ChildItem temp -Recurse
```

Expected contents:

```text
[Content_Types].xml
README.md
YourName.MyMcpServer.nuspec
.mcp\server.json
tools\net10.0\any\DotnetToolSettings.xml
```

Open `YourName.MyMcpServer.nuspec` and confirm it declares **both** package
types:

```xml
<packageTypes>
  <packageType name="DotnetTool" />
  <packageType name="McpServer" />
</packageTypes>
```

If `McpServer` is missing, or `.mcp\server.json` isn't in the package, something
in the `.csproj` was overwritten. Go back to Step 1.

## Step 4: Publish to NuGet.org

### Prerequisites for Publishing

1. **NuGet.org Account**: Create at [nuget.org](https://nuget.org)
2. **API Key**: Generate from your NuGet account settings
3. **Unique Package Name**: Verify your package ID is available

### Publishing Process

1. **Publish the packages**:

   Push every `.nupkg` in the output folder, not just the base one. Consumers on
   a platform whose RID package is missing can't run your server.

   ```powershell
   dotnet nuget push "bin/Release/*.nupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate
   ```

2. **Verify publication**:
   - Check [nuget.org](https://nuget.org) for your package
   - Test installation: `dnx YourName.MyMcpServer`

### Publication Checklist

- ✅ Package builds successfully
- ✅ All metadata is complete and accurate
- ✅ README documentation is comprehensive
- ✅ License is specified
- ✅ Version number follows [SemVer](https://semver.org)
- ✅ Package has been tested locally
- ✅ Dependencies are correctly specified

## Step 5: Consumer Configuration Examples

### VS Code MCP Configuration

**`.vscode/mcp.json`**:

```json
{
  "servers": {
    "MyMcpServer": {
      "type": "stdio",
      "command": "dnx", 
      "args": [
        "YourName.MyMcpServer",
        "--version",
        "1.0.0",
        "--yes"
      ],
      "env": {
        "WEATHER_UNITS": "celsius"
      }
    }
  }
}
```

### Visual Studio 2026 Configuration

**`.mcp.json`** (in solution directory):

```json
{
  "servers": {
    "MyMcpServer": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "YourName.MyMcpServer", 
        "--version",
        "1.0.0"
      ],
      "env": {
        "WEATHER_UNITS": "celsius"
      }
    }
  }
}
```

### Enterprise Configuration

**Private NuGet feed**:

```json
{
  "servers": {
    "MyMcpServer": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "CompanyName.MyMcpServer",
        "--source",
        "https://pkgs.dev.azure.com/company/_packaging/mcp-servers/nuget/v3/index.json",
        "--version", 
        "1.0.0"
      ]
    }
  }
}
```

## Step 6: Version Management

### Semantic Versioning Strategy

Follow [SemVer](https://semver.org) principles:

- **MAJOR** (1.0.0 → 2.0.0): Breaking changes to tool interfaces
- **MINOR** (1.0.0 → 1.1.0): New tools or features, backward compatible
- **PATCH** (1.0.0 → 1.0.1): Bug fixes, performance improvements

### Example Version Updates

**1.0.0** → **1.1.0**: Add new `GetWeatherAlerts` tool

```xml
<Version>1.1.0</Version>
```

**1.1.0** → **1.1.1**: Fix temperature conversion bug

```xml
<Version>1.1.1</Version>
```

**1.1.1** → **2.0.0**: Change tool parameter names (breaking change)

```xml
<Version>2.0.0</Version>
```

### Maintaining Multiple Versions

Support strategy for published packages:

- **Latest Version**: Active development and new features
- **Previous Major**: Security fixes and critical bugs only
- **Legacy Versions**: End-of-life after 12 months

## Step 7: Security Best Practices

### Secure Package Publishing

```xml
<!-- Sign packages for authenticity -->
<PropertyGroup>
  <SignPackage>true</SignPackage>
  <PackageCertificateKeyFile>certificate.pfx</PackageCertificateKeyFile>
</PropertyGroup>
```

### Runtime Security

```csharp
public class WeatherTools
{
    [McpServerTool]
    public async Task<string> GetCurrentWeather(string city)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City name is required");
            
        // Sanitize inputs
        city = city.Trim();
        if (city.Length > 100)
            throw new ArgumentException("City name too long");
            
        // Rate limiting (implement based on your needs)
        await ApplyRateLimit();
        
        // Secure API calls
        using var httpClient = CreateSecureHttpClient();
        
        // Safe data processing
        return await ProcessWeatherDataSafely(city);
    }
}
```

### Configuration Security

```json
{
  "servers": {
    "MyMcpServer": {
      "type": "stdio",
      "command": "dnx",
      "args": ["YourName.MyMcpServer"],
      "env": {
        "WEATHER_API_KEY": "${env:WEATHER_API_KEY}"
      }
    }
  }
}
```

## Step 8: Monitoring and Analytics

### Usage Analytics

```csharp
public class WeatherTools
{
    private readonly ILogger<WeatherTools> _logger;
    private readonly ITelemetryClient _telemetry;
    
    [McpServerTool]
    public async Task<string> GetCurrentWeather(string city)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await ProcessWeatherRequest(city);
            
            _telemetry.TrackEvent("WeatherRequest", new Dictionary<string, string>
            {
                ["Tool"] = "GetCurrentWeather",
                ["City"] = HashCity(city), // Hash for privacy
                ["Duration"] = stopwatch.ElapsedMilliseconds.ToString()
            });
            
            return result;
        }
        catch (Exception ex)
        {
            _telemetry.TrackException(ex);
            throw;
        }
    }
}
```

### Health Monitoring

```csharp
[McpServerTool]
[Description("Health check endpoint for monitoring")]
public async Task<string> HealthCheck()
{
    return JsonSerializer.Serialize(new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
        Uptime = Environment.TickCount64
    });
}
```

## Step 9: Advanced Distribution Scenarios

### Enterprise Distribution

For enterprise environments, consider:

#### Private NuGet Feeds

```xml
<!-- NuGet.config for private feeds -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="CompanyMCP" value="https://pkgs.dev.azure.com/company/_packaging/mcp/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

#### Automated Publishing Pipeline

```yaml
# Azure DevOps pipeline example
trigger:
  tags:
    include:
      - v*

pool:
  vmImage: 'windows-latest'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Build Package'
  inputs:
    command: 'pack'
    packagesToPack: '**/MyMcpServer.csproj'
    configuration: 'Release'

- task: NuGetCommand@2
  displayName: 'Publish to Private Feed'
  inputs:
    command: 'push'
    packagesToPush: '$(Build.ArtifactStagingDirectory)/**/*.nupkg'
    nuGetFeedType: 'internal'
    publishVstsFeed: 'mcp-servers'
```

### Container Distribution

```dockerfile
# Dockerfile for containerized MCP server
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MyMcpServer.csproj", "."]
RUN dotnet restore "MyMcpServer.csproj"
COPY . .
RUN dotnet build "MyMcpServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyMcpServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyMcpServer.dll"]
```

## Step 10: Documentation and Support

### Complete Documentation Strategy

1. **Package README**: Quick start and basic usage
2. **GitHub Wiki**: Detailed documentation and examples
3. **API Documentation**: Generated from code comments
4. **Video Tutorials**: Screen recordings of setup and usage
5. **Sample Projects**: Complete working examples

### Community Support Channels

- **GitHub Issues**: Bug reports and feature requests
- **Discussions**: Community Q&A and sharing
- **Discord/Slack**: Real-time community support
- **Blog Posts**: Tutorials and best practices

### Documentation Templates

**Issue Template**:

```markdown
## Bug Report

**Describe the bug**
A clear description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:
1. Configure MCP server with...
2. Ask AI assistant...
3. See error

**Expected behavior**
What you expected to happen.

**Environment**
- OS: [e.g. Windows 11]
- .NET Version: [e.g. 8.0]
- VS Code Version: [e.g. 1.85]
- Package Version: [e.g. 1.0.0]
```

## Key Concepts Learned

### Publishing Lifecycle

- **Development**: Create and test MCP servers locally
- **Packaging**: Configure metadata and build NuGet packages
- **Publishing**: Distribute through NuGet.org or private feeds
- **Maintenance**: Version management and ongoing support

### Professional Standards

- **Documentation**: Comprehensive guides for consumers
- **Versioning**: Semantic versioning for compatibility
- **Security**: Safe handling of data and inputs
- **Monitoring**: Usage tracking and health checks

### Distribution Strategies

- **Public NuGet**: Wide accessibility and discoverability
- **Private Feeds**: Enterprise control and security
- **Container Distribution**: Consistent deployment environments
- **Source Distribution**: Maximum transparency and customization

## Taking this further

You have completed the MCP workshop series. You now have the foundation to:

### Immediate Next Steps

1. **Publish your own MCP server** using the skills from this workshop
2. **Join the MCP community** and share your creations
3. **Explore advanced scenarios** like multi-tool orchestration
4. **Build business-specific MCP servers** for your organization

### Advanced Learning Paths

- **Enterprise Integration**: Connect MCP to existing business systems
- **Performance Optimization**: Scale MCP servers for high-volume usage
- **Security Hardening**: Implement advanced security patterns
- **AI Agent Development**: Build custom AI agents that consume MCP servers

### Community Contribution

- **Open Source Projects**: Contribute to MCP-related projects
- **Best Practices**: Share patterns and practices with the community
- **Teaching**: Help others learn MCP through tutorials and workshops
- **Innovation**: Pioneer new use cases and applications

## Resources for Continued Learning

### Official Documentation

- 📖 [Model Context Protocol Specification](https://spec.modelcontextprotocol.io/)
- 🔧 [MCP .NET SDK Documentation](https://aka.ms/nuget/mcp/guide)
- 💻 [VS Code MCP Integration](https://code.visualstudio.com/docs/copilot/chat/mcp-servers)

### Community Resources

- 🐙 [MCP GitHub Organization](https://github.com/modelcontextprotocol)
- 💬 [MCP Community Discussions](https://github.com/modelcontextprotocol/specification/discussions)
- 📚 [Awesome MCP](https://github.com/modelcontextprotocol/awesome-mcp)

### Development Tools

- 🛠️ [MCP Inspector](https://github.com/modelcontextprotocol/inspector) - Debug MCP servers
- 📦 [MCP TypeScript SDK](https://github.com/modelcontextprotocol/typescript-sdk)
- 🐍 [MCP Python SDK](https://github.com/modelcontextprotocol/python-sdk)

---

## Summary

In Part 7, you covered the MCP server lifecycle from development to production. You now know how to:

- ✅ Package MCP servers for professional distribution
- ✅ Publish to NuGet with proper metadata and documentation
- ✅ Implement version management and security best practices
- ✅ Support enterprise distribution scenarios
- ✅ Monitor and maintain published MCP servers

You now have a complete path for building, publishing, and maintaining MCP servers that extend AI agents with custom capabilities.

## MCP Track Complete! 🎉

🏆 **Completed the MCP track** of the AI Workshop series:

**AI Web Chat Application (Parts 1-4, 10, and 11)**:

- Built a complete AI-powered web application
- Integrated with Azure OpenAI services
- Deployed to Azure cloud infrastructure

**Model Context Protocol Servers (Parts 5-7)**:

- Created custom MCP servers that extend AI agents
- Implemented both basic and advanced business tools
- Learned professional publishing and distribution

## What's next

**Part 8** is a short bridge module on how chat, tools, and memory come together
as an agent, and how the MCP servers you just built get consumed by your own
agents.

**Continue to** → [Part 8: Agent Framework Essentials](../Part%2008%20-%20Agent%20Framework%20Basics/README.md)

### Share Your Success

🌟 **Built something amazing?** Share your MCP servers and AI applications with the community!

---

📖 **Return to**: [Workshop Overview](../README.md) | 🔄 **Previous**: [Part 6: Enhanced MCP Server](../Part%2006%20-%20Enhanced%20MCP%20Server/README.md) | ➡️ **Next**: [Part 8: Agent Framework Essentials](../Part%2008%20-%20Agent%20Framework%20Basics/README.md)
