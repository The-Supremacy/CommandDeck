# Identity Module

Identity owns template-level local identity and application authorization
behavior.

Identity owns:

- Local user aggregates mapped to OIDC subjects.
- Lazy local user creation after successful authentication.
- Application-owned application access aggregates.
- Current-user context contracts.
- Explicit initial-admin setup behavior.

The Host owns OIDC challenge/callback/logout mechanics, cookie configuration,
Redis-backed session ticket storage, and API authentication behavior.

Identity-provider roles, groups, and organization membership are not
authoritative product authorization sources.

## Current Implementation Slice

Identity follows the backend module pattern described in the server
architecture docs. It defines provider-neutral current-user contracts, local
user aggregates, application access aggregates, Mediator command handlers, and
module-owned repository contracts. A valid authenticated principal with a stable
provider subject lazily creates or updates one local user by `(provider,
subject)`.

Application access is represented by active application-owned access state.
Missing or inactive access means the user is authenticated without product
access; they do not become unauthenticated. An explicit setup command can grant
one configured provider/subject pair active application access and is
intentionally idempotent. Revoked access is sticky unless setup is run with
force.

The Host owns claim parsing. Identity current-user behavior consumes a
provider-neutral authenticated identity value and resolves local user plus
application access state from application-owned data.

Identity currently records these stable domain event types:

- `identity.local-user-created`
- `identity.local-user-seen`
- `identity.application-access-granted`
- `identity.application-access-revoked`
