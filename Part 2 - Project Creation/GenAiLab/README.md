# AI Chat with Custom Data

This project is an AI chat application that demonstrates how to chat with custom data using an AI language model. Please note that this template is currently in an early preview stage. If you have feedback, please take a [brief survey](https://aka.ms/dotnet-chat-templatePreview2-survey).

>[!NOTE]
> Before running this project you need to configure the API keys or endpoints for the providers you have chosen. See below for details specific to your choices.

### Known Issues

#### Errors running Ollama or Docker

A recent incompatibility was found between Ollama and Docker Desktop. This issue results in runtime errors when connecting to Ollama, and the workaround for that can lead to Docker not working for Aspire projects.

This incompatibility can be addressed by upgrading to Docker Desktop 4.41.1. See [ollama/ollama#9509](https://github.com/ollama/ollama/issues/9509#issuecomment-2842461831) for more information and a link to install the version of Docker Desktop with the fix.

# Configure the AI Model Provider

## Using GitHub Models
To use models hosted by GitHub Models, you will need to create a GitHub personal access token with **Models: Read-only** permission. To create the token:

1. Navigate to the [Personal access tokens page](https://github.com/settings/personal-access-tokens/new) of your GitHub account settings under Developer Settings.
2. Select "Generate new token" (fine-grained token).
3. Enter a name for the token, and under **Permissions**, set **Models** to **Access: Read-only**.
4. Select "Generate token" at the bottom of the page.
5. Copy the token for use in the steps ahead.

For more details, see the [Microsoft documentation quickstart](https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/ai-templates?tabs=dotnet-cli%2Cconfigure-visual-studio%2Cconfigure-visual-studio-aspire&pivots=github-models#configure-access-to-github-models) and [Managing your personal access tokens](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens).

From the command line, configure your token for this project using .NET User Secrets by running the following commands:

```sh
cd GenAiLab.AppHost
dotnet user-secrets set ConnectionStrings:openai "Endpoint=https://models.inference.ai.azure.com;Key=YOUR-API-KEY"
```

Learn more about [prototyping with AI models using GitHub Models](https://docs.github.com/github-models/prototyping-with-ai-models).

## Setting up a local environment for Qdrant
This project is configured to run Qdrant in a Docker container. Docker Desktop must be installed and running for the project to run successfully. A Qdrant container will automatically start when running the application.

Download, install, and run Docker Desktop from the [official website](https://www.docker.com/). Follow the installation instructions specific to your operating system.

Note: Qdrant and Docker are excellent open source products, but are not maintained by Microsoft.

# Running the application

## Using Visual Studio

1. Open the `.sln` file in Visual Studio.
2. Press `Ctrl+F5` or click the "Start" button in the toolbar to run the project.

## Using Visual Studio Code

1. Open the project folder in Visual Studio Code.
2. Install the [C# Dev Kit extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) for Visual Studio Code.
3. Once installed, Open the `Program.cs` file in the GenAiLab.AppHost project.
4. Run the project by clicking the "Run" button in the Debug view.

## Trust the localhost certificate

Several Aspire templates include ASP.NET Core projects that are configured to use HTTPS by default. If this is the first time you're running the project, an exception might occur when loading the Aspire dashboard. This error can be resolved by trusting the self-signed development certificate with the .NET CLI.

See [Troubleshoot untrusted localhost certificate in Aspire](https://learn.microsoft.com/dotnet/aspire/troubleshooting/untrusted-localhost-certificate) for more information.

# Updating JavaScript dependencies

This template leverages JavaScript libraries to provide essential functionality. These libraries are located in the wwwroot/lib folder of the GenAiLab.Web project. For instructions on updating each dependency, please refer to the README.md file in each respective folder.

# Learn More
To learn more about development with .NET and AI, check out the following links:

* [AI for .NET Developers](https://learn.microsoft.com/dotnet/ai/)
