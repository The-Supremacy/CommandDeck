# Web Architecture

The frontend is a pnpm workspace with React, Vite, TanStack Query, TanStack
Router, Tailwind, and shared packages for auth helpers, configuration, and
generated API clients.

- `web/apps/admin` is the app-owned administration portal.
- `web/apps/web` is the neutral user-facing portal.
- `web/packages/auth` owns browser-safe BFF session helpers, current-user
  loading, access-state utilities, app-facing TanStack Query composition, and
  the domain-neutral browser-session smoke surface. Host API calls from this
  package should use generated client operations when they exist.
- `web/packages/api-client` owns the generated TypeScript client for
  template-owned Host API endpoints. Generated files live under
  `src/generated/`; hand-written same-origin configuration and exports stay
  outside that folder.
- `web/packages/config` owns shared Vite, Vitest, and TypeScript configuration
  used by frontend packages and apps.
- Browser code calls same-origin BFF/API endpoints and does not store identity
  provider access or refresh tokens.
- Browser login uses same-origin navigation to `/auth/login`; browser logout
  submits a same-origin `POST /auth/logout`.
- Local Vite apps proxy `/api/` and `/auth/` to the Host. Set
  `VITE_HOST_ORIGIN` to override the default Host target of
  `http://localhost:5162`.

## Generated API Clients

Refresh the Host OpenAPI document and frontend generated client with:

```sh
pnpm api-client:generate
```

Check that generated output is current with:

```sh
pnpm api-client:check
```

The generated client defaults browser API calls to the current browser origin
and the `/api/` route space. It must not be configured with identity-provider
origins, provider access tokens, refresh tokens, or provider authorization
payloads.

The local pre-commit hook and default CI workflow run `pnpm api-client:check`
so OpenAPI/client drift is caught before merge.

Hey API also supports TanStack Query generation. The template currently
generates SDK/types only and keeps app-facing query composition in
`web/packages/auth`. The generated-client configuration does not enable the
TanStack Query plugin:

```ts
plugins: ["@tanstack/react-query"];
```

App code consumes template-owned query helpers rather than generated TanStack
Query helpers directly.
