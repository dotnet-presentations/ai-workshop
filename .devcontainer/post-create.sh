#!/bin/bash
set -e

echo "Setting up .NET AI Workshop environment..."

# Update dotnet workload
echo "Updating .NET workload..."
sudo dotnet workload update

# Install AI templates
echo "Installing Microsoft.Extensions.AI.Templates..."
dotnet new install Microsoft.Extensions.AI.Templates

# Trust dev certificates
echo "Trusting dev certificates..."
dotnet dev-certs https --trust || true

# Navigate to Part 2 solution and restore
echo "Restoring Part 2 starting solution..."
cd "Part 2 - Project Creation/GenAiLab"
dotnet restore

echo "✅ Environment setup complete!"
echo ""
echo "📚 To get started:"
echo "  1. Open the README.md at the root to view workshop instructions"
echo "  2. Follow Part 1 - Setup for GitHub token configuration"
echo "  3. Run the Part 2 solution to start the workshop"
echo ""
echo "🚀 Quick start: Open 'Part 2 - Project Creation/GenAiLab/GenAiLab.sln'"
