---
mode: agent
description: Testing procedure for workshop completion. This includes verifying code correctness, documentation clarity, and overall user experience. It also creates an updated /complete directory with the final code.
---

You are an intelligent developer who is completing this workshop. You will step through the tasks outlined in /lab/ from part1 to part5, stopping before the Azure deployment section in part5. You will ask clarifying questions if any requirements are unclear, and prompt for any tokens or configuration as required.

You will create a markdown file documenting your progress and any issues encountered. This markdown file will include a section for any recommended improvements to the documentation regarding any parts which are unclear or for which you had to ask for clarification.

## Purposes of this testing procedure

There are two main objectives:

1. To update the /complete directory with the completed code after workshop or template updates.
2. To identify and document any issues or challenges encountered during the workshop.

## Important Notes for Testing

1. **GitHub Token**: When prompted for a GitHub token, create a classic token with `models` scope (not `github_models:read` as some docs may indicate).

2. **Azure AI Preferred**: Azure AI setup is significantly simpler than GitHub Models. If Azure AI credentials are available, use them instead of GitHub Models for smoother testing experience.

3. **Azure Instructions**: Skip any instructions specifically marked for "BUILD 2025 LAB ATTENDEES" as these are conference-specific.

4. **Code Inconsistencies**: Some documentation may show code that doesn't match the current template. Focus on what actually gets generated and works, not exact code matches in documentation.
   - Documentation shows `SemanticSearchRecord` but actual code uses `IngestedChunk`
   - Documentation shows `IVectorStore` pattern but actual code uses `VectorStoreCollection`

5. **Database Setup**: Part 4 uses PostgreSQL for products database (not SQLite as some documentation may indicate).

6. **Infrastructure Complexity**: Local development setup is more complex than indicated in time estimates:
   - Actual setup time: 45+ minutes (vs 15 minutes estimated)
   - Requires 8+ package additions and 6+ configuration changes
   - Multiple build troubleshooting cycles may be needed
   - PostgreSQL + Qdrant dual-database coordination adds significant complexity

7. **Build Issues**: Static asset conflicts and duplicate file errors are common during Part 4. Document all build errors and resolution steps.

8. **Deployment Validation**: If possible, test actual Azure deployment. Azure deployment is remarkably smooth (~6 minutes total) despite local complexity.

## Testing Discoveries and Improvements

Based on comprehensive testing including real Azure deployment:

### Infrastructure Complexity Issues
- **Local Development**: PostgreSQL + Qdrant coordination creates significant setup friction
- **Actual Time**: 45+ minutes for Part 4 (vs 15 minutes estimated)
- **Package Count**: 8+ additional packages required
- **Build Issues**: Static asset conflicts require troubleshooting

### What Works Excellently
- **Azure Deployment**: Remarkably smooth (~6 minutes total) via Azure Developer CLI
- **Azure AI Integration**: Much simpler than GitHub Models setup
- **Aspire Experience**: Excellent development and deployment experience
- **Educational Content**: AI concepts and implementation patterns are solid

### Recommended Improvements
1. **Prioritize Azure AI**: Simpler setup than GitHub Models
2. **Qdrant-Only Approach**: Would eliminate ~70% of infrastructure complexity
3. **Realistic Time Estimates**: Update lab timing expectations
4. **Troubleshooting Guide**: Include common build issue solutions

### Updated Testing Guidelines
- Use Azure AI credentials when available (simpler than GitHub Models)
- Document all build errors and resolution steps meticulously
- Test actual deployment when possible for complete validation
- Focus on educational value vs infrastructure complexity
- Validate that local complexity doesn't overshadow AI learning objectives

### Post-Testing Improvement Process
After completing the workshop testing, follow these phases for repository improvement:

#### Phase 1: Code Alignment & Documentation Updates
1. **Update `/src/complete`**: Compare with tested `/src/start` end state and sync any differences
2. **Update `test-workshop.prompt.md`**: Add testing insights and discoveries
3. **Update Lab Documentation**: Fix inconsistencies, add realistic time estimates, include troubleshooting

#### Phase 2: Repository Cleanup & Commit Preparation  
4. **Clean Up Testing Artifacts**: Remove backup directories and deployment artifacts
5. **Reset `/src/start`**: Use `git restore src/start/` to revert to original template state
   - Remove untracked files created during testing (`azure.yaml`, `.gitignore`, etc.)
   - Verify build success and pristine state

#### Repository State Verification
- Keep: Documentation improvements, `/src/complete` updates, testing insights
- Revert: All `/src/start` modifications (students should start fresh)
- Ensure: Only intentional improvements remain for commit

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

### Critical Testing Notes Based on Experience

**Part 2**: After configuring GitHub token, test application runtime. Expect initial setup complexity.

**Part 3**: If Azure AI credentials available, prefer them over GitHub Models for simpler setup and better error handling.

**Part 4**: This is the most complex part - allow 45-60 minutes minimum. Common issues:
- Build errors from static asset conflicts: `dotnet clean && dotnet build`
- Package restore problems: `dotnet restore && dotnet build`  
- Multiple containers coordination complexity (PostgreSQL + Qdrant + Web)
- 8+ package additions and 6+ configuration changes required

**Part 5**: If possible, test actual Azure deployment for complete validation. Deployment is remarkably smooth (~6 minutes) despite local complexity.

**Documentation Alignment**: Focus on what actually works vs. exact documentation matches. Current inconsistencies include:
- Documentation shows `SemanticSearchRecord` but code uses `IngestedChunk`
- Documentation shows `IVectorStore` but code uses `VectorStoreCollection`

## Evaluation

1. At the completion of each part, verify that the generated code matches the expected output in the /src/complete directory. Focus on file structure, key configuration files, and core functionality rather than exact line-by-line matches.
2. Document any issues or challenges encountered in the markdown file, including any clarifications needed for the instructions, missing prerequisites, or unclear steps.
3. Note any places where you had to make assumptions or decisions not explicitly covered in the documentation.
4. There will be a manual evaluation after the workshop which will use source control to compare both the start and end states of /src/start and the /src/complete directory.

## Reproducible Commands for Common Issues

### Build Troubleshooting (Part 4)
```bash
# Clean build artifacts if static asset conflicts occur
dotnet clean
dotnet build

# Package restore if needed
dotnet restore
dotnet build
```

### Git-Based Cleanup (Post-Testing)
```bash
# Check what was modified during testing
git status

# Revert /src/start to original state (most efficient approach)
git restore src/start/

# Remove untracked Azure CLI artifacts
Remove-Item "src/start/.gitignore" -ErrorAction SilentlyContinue
Remove-Item "src/start/azure.yaml" -ErrorAction SilentlyContinue  
Remove-Item "src/start/next-steps.md" -ErrorAction SilentlyContinue

# Verify clean state
git status
```

### Testing Validation Commands
```bash
# Verify template build
cd src/start && dotnet build --verbosity minimal

# Check file structure expectations
ls src/start/GenAiLab.Web/Components/Pages  # Should only have Chat/ and Error.razor initially
ls src/start/GenAiLab.Web/Services          # Should not have ProductService.cs initially
```

### Azure Deployment Testing (If Credentials Available)
```bash
# Update azd if needed
winget upgrade Microsoft.Azd

# Initialize for deployment
azd init
# Follow prompts: "Use code in current directory" → "Confirm" → "Environment name"

# Provision and deploy (if testing real deployment)
azd provision  # ~4 minutes
azd deploy     # ~2 minutes

# View results
azd show

# Cleanup when done
azd down --purge --force
```