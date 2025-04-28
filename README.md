# LAB307 - Building GenAI Apps in C#: AI Templates, GitHub Models, Azure OpenAI &amp; More

Get up to speed quickly with AI app building in .NET! Explore the new .NET AI project templates integrated with Microsoft Extensions for AI (MEAI), GitHub Models, and vector data stores. Learn how to take advantage of free GitHub Models in development, then deploy with global scale and enterprise support using Azure OpenAI. Gain hands-on experience building cutting-edge intelligent solutions with state-of-the-art frameworks and best practices.

## Prerequisites

- Visual Studio 2022 with .NET Aspire workload installed
- .NET AI Web Chatbot template installed
- .NET 9.0 SDK or later
- Azure OpenAI subscription (optional, but recommended for full experience)
- GitHub Copilot subscription (optional, but recommended for full experience)

## Lab Overview

The example application consists of two main projects is based on the new .NET AI project templates. It includes:

- **AI Chatbot**: A simple chatbot that can answer questions and provide information based on a information in PDF product manuals.
- **Qdrant vector database**: A vector database that stores the embeddings of the product manuals and allows for efficient similarity search.
- **Azure OpenAI**: A service that provides access to OpenAI's models, including GPT-3 and DALL-E.
- **.NET Aspire AppHost and ServiceDefaults**: A framework for hosting .NET applications with default configurations for services.

## Lab Parts

1. [Setup](lab/setup.md)
1. [Microsoft Extensions for AI (MEAI) fundamentals](lab/part1-meai.md)
1. [Exploring the Codebase in a new AI Web Chat project](lab/part2-exploring-codebase.md)
1. [Vector data and embeddings](lab/part3-vector-data.md)
1. [Using GitHub Models](lab/part4-github-models.md)
1. [Using Azure OpenAI](lab/part5-azure-openai.md)
1. [Deploying the application](lab/part6-deploying.md)

**Key Takeaway**: Microsoft Extensions for AI (MEAI) is a new set of libraries and tools that make it easier to build AI applications in .NET. It provides a consistent programming model, a set of common components, and a set of best practices for building AI applications. The new .NET AI project templates provide a great starting point for building AI applications in .NET.
