# Server

Backend projects live here.

Backend project layout:

- `src/CommandDeck.Host`
- `src/CommandDeck.Migrator`
- `src/CommandDeck.Persistence`
- `src/CommandDeck.ServiceDefaults`
- `src/CommandDeck.SharedKernel`
- `src/modules/CommandDeck.Identity.Contracts`
- `src/modules/CommandDeck.Identity`
- `src/modules/CommandDeck.Identity.Infrastructure`

These projects provide the backend foundation: Host composition,
ServiceDefaults, shared persistence, SharedKernel primitives, Migrator wiring,
and the initial Identity module boundary.

`CommandDeck.Persistence` contains the concrete EF Core DbContext shell.
`CommandDeck.Migrator` is the Host-owned migration entrypoint. The template
does not include generated EF migrations.
