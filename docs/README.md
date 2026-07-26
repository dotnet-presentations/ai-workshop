# Workshop Documentation

This directory contains all documentation related to the .NET AI Workshop development and instruction.

## Documentation Structure

### 📋 Active Planning Documentation (`/planning`)

- **[MCP_TESTING_GUIDE.md](planning/MCP_TESTING_GUIDE.md)** - Comprehensive testing procedures and validation guidelines for MCP functionality

### 🧪 Workshop Test Reports (`/testing`)

New test reports are written here using the template and skill at [`.github/skills/workshop-testing/`](../.github/skills/workshop-testing/SKILL.md). Name files `workshop-test-report-<YYYY-MM-DD>.md`.

### 🗄️ Archived Historical Artifacts (`/archive`)

- **[MCP_WORKSHOP_PLANNING.md](archive/planning/MCP_WORKSHOP_PLANNING.md)** - Legacy implementation plan retained for historical context
- **[workshop-test-report.md](archive/testing/workshop-test-report.md)** - Legacy workshop test report (Aug 2025)
- **[workshop-test-report-2025-10-02.md](archive/testing/workshop-test-report-2025-10-02.md)** - Legacy workshop test report (Oct 2025)

### 👨‍🏫 Instructor Resources (`/instructor`)

- **[ATTENDEE_AI_ACCESS.md](instructor/ATTENDEE_AI_ACCESS.md)** - Instructor-only note on providing an AI access fallback (azure-ai-proxy-lite) for attendees who can't provision their own [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) resource
- **[MCP_INSTRUCTOR_GUIDE.md](instructor/MCP_INSTRUCTOR_GUIDE.md)** - Complete instructor guide for teaching Parts 5-7 (MCP components) of the workshop
- **[END_OF_WORKSHOP_RESOURCES_SLIDE.md](instructor/END_OF_WORKSHOP_RESOURCES_SLIDE.md)** - Closing slide content for end-of-workshop resource handoff

## Quick Navigation

### For Workshop Developers

- Use [MCP_TESTING_GUIDE.md](planning/MCP_TESTING_GUIDE.md) for testing and validation procedures
- Use archived docs only for historical reference, not current implementation steps

### For Workshop Instructors

- Review [MCP_INSTRUCTOR_GUIDE.md](instructor/MCP_INSTRUCTOR_GUIDE.md) for complete teaching guidance on MCP components
- Reference the planning documentation for technical implementation details

## Workshop Overview

The workshop is structured as 11 parts:

**Core AI application development (Parts 1-4):**

- Part 1: Setup and Prerequisites
- Part 2: Build Chat App
- Part 3: Add RAG
- Part 4: AI Web Chat Template

**Model Context Protocol Extension (Parts 5-7):**

- Part 5: MCP Server Basics
- Part 6: Enhanced MCP Server *(optional / bonus)*
- Part 7: MCP Publishing *(optional / bonus)*

**Agents, capstone, and deployment (Parts 8-11):**

- Part 8: Agent Framework Essentials
- Part 9: Adding AI to an Existing App
- Part 10: Choosing Providers and Services
- Part 11: Azure Deployment

## Contributing

When adding new documentation:

- Place planning and development docs in `/planning`
- Place instructor and teaching materials in `/instructor`
- Place new workshop test reports in `/testing` (use the [skill + template](../.github/skills/workshop-testing/SKILL.md))
- Place historical/obsolete docs in `/archive`
- Update this README.md to include new documents
- Use descriptive filenames and maintain consistent formatting
