# Dependency version policy

This document is the source of truth for dependency versions that should not be
updated independently. It exists for maintainers and reviewers; attendee READMEs
should still explain any pin that attendees must preserve while completing a lab.

The committed projects use exact package versions so workshop snapshots remain
reproducible. An exact version is not automatically a protected pin. Most package
updates are acceptable after the snapshot, its README, and the affected behavior
have been validated together.

## Constraint types

- **Security override:** a direct reference promotes a safe transitive version.
  Keep it until the parent dependency resolves a safe version without the override.
- **Compatibility boundary:** another package or generated API limits the allowed
  version range. Do not cross the boundary without updating the dependent package
  and code together.
- **Coordinated set:** several packages, projects, or workshop parts describe one
  working state. Move the full set together.
- **Template baseline:** the version records the output or APIs of a template used
  by the lab. Re-scaffold and compare before changing it.

## Active constraints

### Security and compatibility overrides

| Dependency | Scope | Why it is direct or constrained | Revisit when |
| --- | --- | --- | --- |
| `Microsoft.Bcl.Memory` 10.0.11 | Part 3 MEDI checkpoint | Promotes the transitive dependency brought in by tokenizer packages above a version with a high-severity advisory. | A restore without the direct reference resolves a non-vulnerable version. A newer safe patch may replace 10.0.10 after affected builds remain warning-free. |
| `Microsoft.Bcl.Memory` 10.0.11 | Part 11 GenAiLab web app | Promotes the transitive dependency brought in by tokenizer packages above a version with a high-severity advisory. | A restore without the direct reference resolves a non-vulnerable version. A newer safe patch may replace 10.0.11 after affected builds remain warning-free. |
| `SQLitePCLRaw.bundle_e_sqlite3` 3.0.4 | Part 3 MEDI checkpoint; both Part 9 Products projects; Part 4 Docker-free instructions | Promotes the native SQLite bundle above the vulnerable transitive version used by SqliteVec or the local template path. | The parent packages resolve a non-vulnerable native bundle without the override. A newer safe patch is allowed after local SQLite and vector-search behavior is tested. |
| `Microsoft.OpenApi` 2.7.5 | Both Part 9 Products projects | Originally added as a security override. It is also bounded by `Microsoft.AspNetCore.OpenApi` 10.0.x, which requires `Microsoft.OpenApi` below 3.0. Updating only this package to 3.x produces `NU1608` and source-generator compile errors. | The selected `Microsoft.AspNetCore.OpenApi` version supports 3.x, or its transitive dependency is safe enough to remove the direct reference. Update the start and completed snapshots together. |

The versions above are the currently verified safe versions, not permanent maximums.
For a security override, prefer the smallest update that keeps the graph free of
known advisories and preserves the lab behavior.

### Coordinated and template-bound versions

| Dependency set | Scope | Constraint | Revisit procedure |
| --- | --- | --- | --- |
| Aspire 13.5.3 packages and AppHost SDK | Parts 4, 10, and 11 GenAiLab progression | Part 4 moves the template from Aspire 13.0.0 because that graph reports a high-severity MessagePack advisory. The SDK and Aspire packages form one tested set, and 13.5.3 requires `AspireUseCliBundle` to avoid `ASPIRE010`. | Update the Part 4 commands, Part 10 provider commands, Part 11 snapshot, and instructor template notes together. Test local orchestration and deployment configuration. |
| Aspire AppHost SDK 13.4.6 | Part 9 `eShopLite-start` and `eShopLite` | This is the baseline inherited by the existing-app scenario. The start and answer projects must remain equivalent before AI changes are applied. It is not a permanent security pin. | Update both AppHost projects together, confirm the starting application still matches the lab, and run the completed AI flows. |
| `Microsoft.Extensions.DataIngestion*` previews | Part 3 MEDI checkpoint and Part 11 GenAiLab | These packages are prerelease and their APIs follow the template's ingestion pipeline. Different parts may intentionally reflect different tested template states. | Re-run the relevant template or checkpoint flow, reconcile API changes, and update instructions and snapshots together. Do not normalize versions across parts solely for consistency. |
| `Aspire.Azure.AI.OpenAI` preview | Part 11 GenAiLab | No stable package exists for the template integration used by this snapshot, and its compatibility is tied to the GenAiLab Aspire set. | Validate against the current AI Web Chat template and the complete GenAiLab dependency set before changing it. |
| `Microsoft.SemanticKernel.Connectors.SqliteVec` preview | Parts 3 and 9 | The workshop relies on prerelease vector-store APIs and on its SQLite transitive graph. | Validate compilation, ingestion/search behavior, and the SQLite security override before changing it. |

## Versions that are not protected pins

Routine exact references such as stable `Microsoft.Extensions.*` packages other
than `Microsoft.Extensions.DataIngestion*`, OpenTelemetry, and patch-level runtime
packages generally record the last tested snapshot. They may be updated, but
related packages should stay on compatible versions and every affected snapshot
must continue to build without warnings.

Major updates are never routine in this repository. For example,
`ModelContextProtocol` 1.x to 2.x, `Microsoft.OpenApi` 2.x to 3.x, or an Agent
Framework release with API changes must be isolated and checked against the lab's
code and teaching flow. Do not group unrelated major updates into one pull request.

## Reviewing a dependency change

Use the [dependency update review skill](../../.github/skills/dependency-update-review/SKILL.md)
to apply this checklist consistently to Dependabot and manual package updates.

1. Classify each update as routine, security override, compatibility boundary,
   coordinated set, or template baseline.
1. Check the affected README for package commands and explanations. Update it when
   the committed snapshot would no longer be produced by following those steps.
1. Keep paired projects aligned, especially Part 9's start and completed snapshots
   and the Parts 4, 10, and 11 GenAiLab progression.
1. Inspect the resolved transitive graph and vulnerability report. Confirm that a
   proposed removal of an override still resolves a safe version.
1. Build every affected CI target in Release with zero warnings. For major,
   prerelease, template, or provider changes, also run the relevant attendee flow.
1. Record why a protected pin changed or was removed in the pull request and update
   this inventory in the same change.
1. After repairing a generated dependency pull request, reconcile its title and
  description with the final diff and resolve review threads only after recording
  the validation evidence.

Use the `workshop-testing` skill when an update requires re-scaffolding a project,
reconciling a snapshot, or exercising the workshop as an attendee.
