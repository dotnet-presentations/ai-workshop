---
mode: agent
---

You are an intelligent developer who is completing this workshop. You will step through the tasks outlined in /lab/ from part1 to part5, stopping before the Azure deployment section in part5. You will ask clarifying questions if any requirements are unclear, and prompt for any tokens or configuration as required.

You will create a markdown file documenting your progress and any issues encountered. This markdown file will include a section for any recommended improvements to the documentation regarding any parts which are unclear or for which you had to ask for clarification.

## Purposes of this testing procedure

There are two main objectives:

1. To update the /complete directory with the completed code after workshop or template updates.
2. To identify and document any issues or challenges encountered during the workshop.

## Procedure

1. Copy the contents of /src/start to a backup directory named /src/start-backup.
2. Delete the contents of /src/start.
3. Follow the steps in /lab/part1.md to create a new solution in the /src/start directory. You have two options for project creation:
   - **Option A (Recommended for automation)**: Use the CLI method described in the "Alternative: Create the project using the .NET CLI" section
   - **Option B**: Use Visual Studio if available
   
   If using the CLI method, note that the `dotnet new aichatweb` command will create a subdirectory with the project name. You'll need to move the contents from the generated subdirectory up to the /src/start directory to match the expected structure.
   
   If the generated structure doesn't match the expected /src/start directory structure (based on /src/complete), reorganize the files to match the expected layout.
4. Carefully step through each part of the workshop, referring to the instructions in /lab/part1.md through /lab/part5.md. Note that some text may describe code but are not actually instructing you to make changes. Stop before any Azure deployment steps in part5.
5. If at any point you need to check the current state of the code, refer to the /src/start and /src/complete directories for comparison.

## Evaluation

1. At the completion of each part, verify that the generated code matches the expected output in the /src/complete directory. Focus on file structure, key configuration files, and core functionality rather than exact line-by-line matches.
2. Document any issues or challenges encountered in the markdown file, including any clarifications needed for the instructions, missing prerequisites, or unclear steps.
3. Note any places where you had to make assumptions or decisions not explicitly covered in the documentation.
4. There will be a manual evaluation after the workshop which will use source control to compare both the start and end states of /src/start and the /src/complete directory.