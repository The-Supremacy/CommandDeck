import { afterEach, describe, expect, it, vi } from "vitest";

import {
  loginPath,
  logoutPath,
  startLogin,
  startLogout,
  type BrowserLocation,
} from "../src";

afterEach(() => {
  document.body.replaceChildren();
  vi.restoreAllMocks();
});

describe("session navigation helpers", () => {
  it("starts login through the Host login route", () => {
    const assignedUrls: string[] = [];
    const location: BrowserLocation = {
      href: "http://localhost:5173/current?tab=me",
      assign(url) {
        assignedUrls.push(url);
      },
    };

    startLogin(location);

    expect(assignedUrls).toEqual([
      `${loginPath}?returnUrl=http%3A%2F%2Flocalhost%3A5173%2Fcurrent%3Ftab%3Dme`,
    ]);
  });

  it("starts logout through a POST to the Host logout route", () => {
    const submit = vi
      .spyOn(HTMLFormElement.prototype, "submit")
      .mockImplementation(() => undefined);

    startLogout();

    const form = document.querySelector("form");
    expect(form).toBeInstanceOf(HTMLFormElement);
    expect(form?.getAttribute("method")).toBe("post");
    expect(form?.getAttribute("action")).toBe(logoutPath);
    expect(submit).toHaveBeenCalledOnce();
  });
});
