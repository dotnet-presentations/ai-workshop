# .NET AI Workshop

Teaching repository for a one-day, 11-part .NET AI workshop. The active workshop
content is in zero-padded `Part NN - <Name>/` folders; prose and headings use the
unpadded "Part N" form. Some parts deliberately continue work from earlier parts.

Everything targets **.NET 10** and uses **Microsoft Foundry (Azure OpenAI)** as the
default provider.

## Repository invariants

- Treat the root `README.md` as the workshop catalog and pacing authority.
- Keep attendee instructions and their committed code snapshots in sync.
- Scaffold projects with `dotnet new`; do not hand-author project files that the
  workshop teaches attendees to generate.
- Never commit secrets or generated output such as `bin/`, `obj/`, `.vs/`, or
  `.azure/`.
- Preserve a zero-warning build. Use `.github/workflows/dotnet-build.yml` as the
  authority for the current build matrix and SDK version.
- Use `.github/workflows/markdownlint.yml` and `.github/workflows/link-check.yml`
  as the authorities for documentation validation.

## Working in this repository

- Read the nearest part README before changing a lab or snapshot.
- Prefer plain, direct, attendee-facing prose. Keep time estimates and the root
  schedule aligned when a part's length changes.
- Follow the path-specific instructions in `.github/instructions/` for workshop
  prose, snapshot code, and the shared GenAiLab progression.
- Use the `workshop-testing` skill for attendee-style walkthroughs, snapshot
  reconciliation, or workshop test reports.
- Consult `docs/instructor/` for volatile template behavior and instructor-only
  implementation context instead of duplicating those details here.
