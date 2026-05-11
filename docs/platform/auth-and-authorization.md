# Auth And Authorization

Authentication mechanics are a Host/platform responsibility. Identity and
application authorization data are module responsibilities.

Host responsibilities:

- Configure ASP.NET Core authentication.
- Handle OIDC challenge, callback, and logout routes.
- Store server-side session tickets in Redis.
- Ensure unauthenticated API requests return `401`.
- Ensure forbidden API requests return `403`.
- Suppress browser redirects for API endpoints.

Identity module responsibilities:

- Translate authenticated OIDC principals into local user identities.
- Store app-owned authorization records.
- Provide current-user context.
- Bootstrap one initial application admin.

The identity provider proves identity. The application decides product access.

## Current Implementation Slice

The template includes Host-owned OIDC and cookie authentication for browser
sessions. The Host uses the application cookie as the local session scheme and
OpenID Connect as the login/sign-out challenge scheme. Authentication ticket
state is stored server-side in Redis through a minimal Host-owned ticket store,
so browser code receives only an opaque application session cookie.

The Host resolves a request principal, maps it to a provider-neutral
authenticated identity value, requires authentication for the current-user
endpoint, and exposes application-access authorization as a Host policy backed
by Identity contracts.

API authentication failures return `401` without browser redirects. API
authorization failures for authenticated users without active application-owned
access return `403`.

Custom request-header authentication is not production authentication. It exists
only in backend tests as temporary verification scaffolding. It must not be
wired into production Host composition, emitted as response state, or used as a
replacement for calling `GET /api/me`.

## Local OIDC Defaults

Local development uses Keycloak through Aspire. The checked-in realm import
configures the `commanddeck` realm and `commanddeck-host` public OIDC
client with PKCE and the Host callback/logout routes.

The development defaults are:

- OIDC authority: `http://localhost:8080/realms/commanddeck`
- OIDC client id: `commanddeck-host`
- Login route: `/auth/login`
- Logout route: `POST /auth/logout`
- Callback route: `/auth/callback`
- Signed-out callback route: `/auth/signed-out`
- Redis connection string key: `ConnectionStrings:session-tickets`

Identity-provider roles, groups, organizations, and provider-specific
authorization claims are not authoritative for product authorization.
