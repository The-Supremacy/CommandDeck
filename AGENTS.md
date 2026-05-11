# Agent Index

This repository is a domain-neutral .NET + React modular-monolith template.

Start with:

- [docs/README.md](docs/README.md) for stable documentation.
- [docs/governance.md](docs/governance.md) for hard project rules.

Do not add domain behavior, auth/session plumbing, generated migrations,
frontend apps, orchestration resources, CI workflows, or template automation
unless accepted feature artifacts or durable architecture decisions state the
scope.

<!-- OPENSPEC START -->

Use [docs/governance.md](docs/governance.md) for hard project rules.
OpenSpec is initialized with Codex skills under `.agents/skills`.
Accepted current behavior is maintained under `openspec/specs/`.
Active changes live under `openspec/changes/`.

Before proposing or implementing substantial runtime behavior, read
`docs/governance.md`, relevant stable docs, and existing OpenSpec current
specs. Archive accepted changes so `openspec/specs/` remains the current source
of truth.

<!-- OPENSPEC END -->
