# AGENTS.md

## Feedback Loops

Run these commands from the repository root.

- `dotnet build -c Release`: compiles the library and the test project.
- `dotnet test -c Release`: runs the unit and integration tests. The integration tests start a PostgreSQL container via Testcontainers, so Docker must be running.

## Guided Coding

This repository uses Guided Coding. Plans and Plan Deviations documents live in `ai-plans/`. See [`ai-plans/AGENTS.md`](ai-plans/AGENTS.md) for the file naming conventions and the rules for working with Frozen Plans.
