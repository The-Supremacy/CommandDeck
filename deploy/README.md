# Deploy

Deployment notes and environment-specific assets may live here once a generated
product chooses deployment targets and artifact publishing strategy.

The template intentionally does not include deployment automation.

Any deployment target that adds a cluster init job, Helm hook,
Terraform-driven task, or pipeline migration step should run
`CommandDeck.Migrator` with `Identity:InitialAdmin:Provider` and
`Identity:InitialAdmin:Subject` supplied as environment configuration. The
Migrator applies schema migrations first, then runs initial-admin setup. Use
`Identity:InitialAdmin:Force=true` only for an intentional reactivation after
revocation.
