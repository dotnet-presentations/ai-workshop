---
description: "Use when changing .NET workshop sample code, project files, packages, configuration, or README steps that produce a committed snapshot."
applyTo: "Part 02 - Build Chat App/**,Part 03 - Add RAG/**,Part 05 - MCP Server Basics/**,Part 06 - Enhanced MCP Server/**,Part 08 - Agent Framework Basics/**,Part 09 - Adding AI to an Existing App/**,Part 11 - Deployment/**"
---

# Workshop code snapshots

- Treat each snapshot as the result of following its lab README, not as an
  independent sample. Keep commands, package references, configuration names,
  source code, and observable behavior aligned in both directions.
- Preserve the workshop's scaffold-first flow. Projects taught as generated must
  begin with the documented `dotnet new` command; record required package updates
  as explicit attendee steps.
- Read the affected project files and
  `docs/instructor/DEPENDENCY_POLICY.md` before changing package versions. Preserve
  security overrides, compatibility boundaries, coordinated sets, and cross-project
  consistency documented by the surrounding lab.
- Store credentials in user secrets or the configuration mechanism taught by the
  lab. Never add real endpoints, keys, generated `.azure/` state, or deployment
  output to a snapshot.
- Do not commit `bin/`, `obj/`, `.vs/`, or `TestResults/` directories.
- Build every affected target in Release and treat warnings as regressions. Use
  `.github/workflows/dotnet-build.yml` for the authoritative SDK and target list.
- Use the `workshop-testing` skill when the task requires reproducing a snapshot
  from the README or validating the lab as an attendee.
