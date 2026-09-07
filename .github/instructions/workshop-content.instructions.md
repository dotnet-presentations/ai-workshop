---
description: "Use when authoring or reviewing attendee workshop READMEs, lab instructions, pacing, or contributor documentation."
applyTo: "README.md,Part */README.md,docs/**/*.md"
---

# Workshop content

- Write for an attendee following the lab in order. Use plain, direct prose and
  make commands runnable as written.
- Use zero-padded folder names in paths (`Part 02 - ...`) and unpadded part names
  in prose and headings (`Part 2`).
- Check the root `README.md` before changing part names, ordering, duration, or
  prerequisites. Keep its schedule aligned with meaningful pacing changes.
- When instructions create or modify a committed sample, update the corresponding
  snapshot in the same change. When snapshot behavior changes, update the README
  that teaches it.
- Keep instructor-only rationale in `docs/instructor/`; attendee READMEs should
  contain only what attendees need to complete and understand the lab.
- Validate changed Markdown using the commands and configuration in
  `.github/workflows/markdownlint.yml` and `.github/workflows/link-check.yml`.
- Use the `workshop-testing` skill for a literal attendee walkthrough, snapshot
  reconciliation, or a workshop test report.
