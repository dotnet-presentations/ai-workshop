# LAB307 - Building GenAI Apps in C#: AI Templates, GitHub Models, Azure OpenAI & More

Get up to speed quickly with AI app building in .NET! Explore the new .NET AI project templates integrated with Microsoft Extensions for AI (MEAI), GitHub Models, and vector data stores. Learn how to take advantage of free GitHub Models in development, then deploy with global scale and enterprise support using Azure OpenAI. Gain hands-on experience building cutting-edge intelligent solutions with state-of-the-art frameworks and best practices.

## Prerequisites

- Visual Studio 2022 with .NET Aspire workload installed
- .NET AI Web Chatbot template installed
- .NET 9.0 SDK or later
- Azure OpenAI subscription (optional, but recommended for full experience)
- GitHub Copilot subscription (optional, but recommended for full experience)

## Lab Overview

The lab consists of a series of hands-on exercises where you'll build an AI-powered web application using the new .NET AI project templates. The application includes:

- **AI Chatbot**: A conversational interface that can answer questions about products
- **Product Catalog**: AI-generated product descriptions and categories
- **Semantic Search**: Vector-based search using document embeddings
- **Integration with GitHub Models and Azure OpenAI**: Use free models for development and enterprise-grade models for production

## Key Technologies

- **.NET 9**: The latest version of .NET
- **Microsoft Extensions for AI (MEAI)**: Libraries for integrating AI capabilities into .NET applications
- **Blazor**: For building interactive web UIs
- **.NET Aspire**: For orchestrating cloud-native distributed applications
- **GitHub Models**: Free AI models for development
- **Azure OpenAI**: Enterprise-grade AI models for production
- **Qdrant Vector Database**: For storing and searching vector embeddings

## Getting Started

Follow the [setup instructions](lab/setup.md) to get started with the lab.

## Lab Modules

The lab is divided into six modules:

1. [**Microsoft Extensions for AI (MEAI) Fundamentals**](lab/part1-meai.md): Explore the structure of an AI Web Chat project and add a Products feature that uses AI to generate descriptions and categories.

2. [**Microsoft Extensions for AI (MEAI) Fundamentals**](lab/part2-exploring-codebase.md#building-the-products-feature): Learn about the core libraries and components for AI development in .NET.

3. [**Vector Data and Embeddings**](lab/part3-vector-data.md): Understand how to use embeddings for semantic search and content matching.

4. [**Using GitHub Models**](lab/part4-github-models.md): Integrate GitHub Models for free AI capabilities during development.

5. [**Using Azure OpenAI**](lab/part5-azure-openai.md): Migrate to Azure OpenAI for enterprise-grade AI capabilities in production.

6. [**Deploying the Application**](lab/part6-deploying.md): Deploy your AI application to production environments.


## Lab Structure

The repository is structured as follows:

- `/lab`: Contains all the lab instructions and documentation
- `/src`: Contains the completed solution that you'll build during the lab

## Additional Resources

- [.NET AI Documentation](https://learn.microsoft.com/en-us/dotnet/machine-learning/ai-overview)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)
- [Microsoft Extensions for AI Documentation](https://learn.microsoft.com/en-us/dotnet/machine-learning/extensions-ai/)
- [Azure OpenAI Documentation](https://learn.microsoft.com/en-us/azure/ai-services/openai/)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
