# Starting Solution for GitHub Codespaces

This folder contains a pre-configured starting solution for users running the workshop in GitHub Codespaces or those who want to skip the initial project creation step.

## What's Included

The `GenAiLab/` solution in this folder contains:

- **GenAiLab.Web**: A Blazor web application with AI chat interface
- **GenAiLab.AppHost**: .NET Aspire orchestration for distributed applications  
- **GenAiLab.ServiceDefaults**: Shared service defaults for Aspire projects

This is the same solution you would create by following [Part 2: Project Creation](../../Part%202%20-%20Project%20Creation/README.md).

## When to Use This

Use this starting solution if:

- 🌐 You're running the workshop in **GitHub Codespaces**
- ⚡ You want to **skip the project creation step** and start directly with configuration
- 🚫 You **can't install Docker** or other prerequisites locally
- 🔧 You have **setup issues** with your local environment

## Getting Started

### In GitHub Codespaces

If you opened this repository in Codespaces, you're already in the right place! The devcontainer configuration automatically:
- Restores NuGet packages for this solution
- Installs required .NET workloads
- Opens this solution in VS Code

To run the application:

```bash
cd src/start/GenAiLab
dotnet run --project GenAiLab.AppHost
```

### Locally

If you want to use this starting solution locally:

1. Ensure you have the prerequisites:
   - .NET 9.0 SDK or later
   - Docker Desktop or Podman
   - Visual Studio 2022 or VS Code

2. Navigate to this folder:
   ```bash
   cd src/start/GenAiLab
   ```

3. Restore packages:
   ```bash
   dotnet restore
   ```

4. Open the solution:
   ```bash
   # In Visual Studio
   start GenAiLab.sln
   
   # Or in VS Code
   code .
   ```

## Next Steps

After getting the starting solution running:

1. **Configure GitHub Models** (or Azure OpenAI):
   - Follow the instructions in [Part 2: Project Creation](../../Part%202%20-%20Project%20Creation/README.md#configure-github-models-authentication)
   
2. **Explore the Template**:
   - Continue with [Part 3: Template Exploration](../../Part%203%20-%20Template%20Exploration/README.md)

3. **Add Features**:
   - Build out the product catalog in [Part 5: Products Page](../../Part%205%20-%20Products%20Page/README.md)

4. **Deploy to Azure**:
   - Learn deployment in [Part 6: Deployment](../../Part%206%20-%20Deployment/README.md)

## Differences from Workshop Instructions

If you're using this starting solution, note these differences:

- ✅ **Skip**: Part 2's project creation steps (already done)
- ✅ **Skip**: Initial `dotnet new` command
- ✅ **Skip**: Visual Studio project wizard
- ▶️ **Start**: Directly at Part 2's "Configure GitHub Models Authentication" section

Everything else in the workshop instructions applies as written.

## Troubleshooting

### Build Errors

If you encounter build errors:

```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

### Missing Workloads

If you see errors about missing workloads:

```bash
sudo dotnet workload update
```

### Port Already in Use

If port 17057 (Aspire dashboard) is already in use:

```bash
# Find and kill the process using the port
lsof -ti:17057 | xargs kill -9
```

## Related Documentation

- [Running in Codespaces](../../.devcontainer/README.md)
- [Part 1: Setup](../../Part%201%20-%20Setup/README.md)
- [Part 2: Project Creation](../../Part%202%20-%20Project%20Creation/README.md)
- [Workshop README](../../README.md)
