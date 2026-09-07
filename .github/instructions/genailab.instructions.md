---
description: "Use when changing GenAiLab, AI Web Chat template instructions, Azure AI Search migration guidance, or Parts 4, 10, and 11."
applyTo: "Part 04 - AI Web Chat Template/**,Part 10 - Choosing Providers and Services/**,Part 11 - Deployment/**,docs/instructor/AICHATWEB_TEMPLATE_NOTES.md"
---

# GenAiLab progression

- Treat Parts 4, 10, and 11 as one app progression. Part 4 scaffolds GenAiLab,
  Part 10 changes its production service choices, and the Part 11 snapshot is the
  deployment-ready result.
- Read `docs/instructor/DEPENDENCY_POLICY.md` before changing package versions in
  this progression. Preserve its security overrides and coordinated Aspire set.
- Preserve the exact Part 4 scaffold command unless intentionally updating the
  entire progression. Its provider, vector-store, Aspire, name, and output options
  determine what later instructions can assume.
- The AppHost deliberately uses the existing Azure OpenAI connection instead of
  provisioning a new account. Preserve that behavior and the corresponding
  attendee step across Parts 4 and 11.
- Keep Azure AI Search as the recommended Part 10/11 production path and Qdrant as
  the documented fallback unless the workshop's provider strategy changes.
- When template output, package requirements, or infrastructure behavior changes,
  verify the current template and update the relevant READMEs, Part 11 snapshot,
  and `docs/instructor/AICHATWEB_TEMPLATE_NOTES.md` together.
- Do not commit `azure.yaml` or `.azure/`; deployment initialization generates
  them at runtime.
