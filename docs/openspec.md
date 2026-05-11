# OpenSpec Usage

OpenSpec is the template's default spec-driven development workflow.

## When To Use It

Use OpenSpec for system behavior that needs durable acceptance criteria,
user-visible semantics, cross-artifact planning, or accepted current-state
capability specs. Narrow documentation, governance, tooling, implementation
structure, and verification-only changes can be handled directly when they do
not change runtime behavior.

Do not add domain behavior, auth/session plumbing, generated migrations,
frontend apps, orchestration resources, CI workflows, generated clients, or
template automation without an accepted OpenSpec change or durable architecture
decision that states the scope.

## Preferred Flow

1. Read [governance.md](governance.md), relevant stable docs, and current specs
   under `openspec/specs/`.
2. Create a change with `openspec new change <name>`.
3. Write `proposal.md`, behavior capability specs under `specs/`, `design.md`
   when the change is cross-cutting, and `tasks.md`.
4. Validate with `openspec validate <change-name> --strict`.
5. Implement and verify the task list.
6. Archive accepted changes with `openspec archive <change-name> --yes` so
   current behavior is merged into `openspec/specs/`.

## Current Specs

Accepted behavior lives under `openspec/specs/`. Technical, governance, and
verification-only migration details may remain in archived changes, but they do
not become active capability specs unless they define observable system
behavior.

Changes modify the existing capability spec that owns the behavior instead of
re-specifying an entire historical feature.

## Tooling

Run [setup-openspec.sh](../scripts/setup-openspec.sh) to install the pinned
OpenSpec CLI and initialize Codex support in a fresh checkout.

OpenSpec is intentionally not installed from devcontainer lifecycle hooks.
