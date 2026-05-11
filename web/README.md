# Web

Frontend apps and packages live here.

Current shape:

- `apps/admin`
- `apps/web`
- `packages/api-client`
- `packages/auth`
- `packages/config`

Future shared packages may include `packages/ui` once its scope is accepted.

Local Vite apps proxy `/api/` and `/auth/` to the Host. Set `VITE_HOST_ORIGIN`
to override the default target of `http://localhost:5162`.
