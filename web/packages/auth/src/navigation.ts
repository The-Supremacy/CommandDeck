export const loginPath = "/auth/login";
export const logoutPath = "/auth/logout";

export interface BrowserLocation {
  readonly href: string;
  assign(url: string): void;
}

export function startLogin(location: BrowserLocation = window.location): void {
  const query = new URLSearchParams({ returnUrl: location.href });

  location.assign(`${loginPath}?${query.toString()}`);
}

export function startLogout(documentRef: Document = window.document): void {
  const form = documentRef.createElement("form");
  form.method = "post";
  form.action = logoutPath;

  documentRef.body.appendChild(form);
  form.submit();
}
