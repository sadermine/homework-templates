# homework-templates

## What this is

Printable homework worksheets generated entirely in the browser. A parent picks a template, sets
options, and prints. There is no backend, no database, and no account, because the whole app is a
Blazor WebAssembly bundle served as static files from GitHub Pages.

The worksheet spec lives in the URL. That is what makes a sheet shareable and reprintable.

## Status

Active. Every project targets `net10.0`. Deployed publicly at
https://sadermine.github.io/homework-templates/ on every push to `main`.

Handles no user data and no credentials. Nothing leaves the browser.

## Commands

```
dotnet run --project src/HomeworkTemplates.Web
dotnet test
```

## Invariants

- **`HomeworkTemplates.Core` stays pure.** No UI types, no Blazor, no browser APIs. It is
  referenced by the test project and must stay testable without a renderer.
- **`WorksheetSpec.TryParse` and `WorksheetSpec.ToQuery` are inverses.** A spec serialized to a
  query string and parsed back must produce the same spec. Breaking this breaks every shared and
  bookmarked link.
- **`TryParse` is the only place untrusted input enters.** It takes raw query values and either
  returns a valid spec or a human-readable error. Missing keys fall back to `Default`, but a key
  that is present and invalid is rejected rather than silently corrected. Do not add a second
  parsing path.
- **Generation is deterministic for a given spec.** `WorksheetGenerator.Generate` seeds `Random`
  from `spec.Seed` and nothing else. The same URL must always print the same sheet, otherwise the
  answer key stops matching the worksheet.
- **Factors stay within `MinFactor` and `MaxFactor` (1 to 20).** Enforced in `TryParse`, not by
  convention at call sites.

## Boundaries

`src/HomeworkTemplates.Web/wwwroot/lib/` is vendored Bootstrap. `tools/viewport-check/` is a
Playwright script under node with its own `node_modules`. Neither is compiled or analyzed.

## Closed decisions

- **Blazor WebAssembly, not Server.** The app must run as static files on GitHub Pages, which
  rules out a server render mode.
- **The spec lives in the query string rather than local storage.** Shareability and reprinting
  matter more than convenience, and it keeps the app stateless.
- **The sheet grid flows column-major in CSS, not by permuting the problem list.**
  `WorksheetGenerator` emits problems in reading order and `SheetStyle.GridTemplate` sets
  `grid-auto-flow: column`. Do not reorder the list to achieve a visual layout.

## Traps

- **Deployment rewrites `<base href>`.** `.github/workflows/deploy.yml` runs `sed` to change
  `/` to `/homework-templates/` in the published `index.html`. Locally the app runs at the root,
  so a base-href bug only ever shows up in production.
- **Deep links depend on `404.html`.** The workflow copies `index.html` to `404.html`, because
  GitHub Pages has no rewrite rules. Without it every route except the root 404s on refresh.
- **`.nojekyll` must exist** or GitHub Pages drops the `_framework` directory and the app fails to
  boot with no useful error.
- **A spec constructed directly, bypassing `TryParse`, can produce an empty pool.**
  `Generate` returns a single empty page rather than throwing. That is deliberate, and it means a
  blank worksheet is a symptom of an invalid spec rather than a rendering fault.

## Out of scope

One template ships today, multiplication. Adding a template means a `TemplateInfo` entry in
`TemplateCatalog.All` and a matching `@page` component, not a new abstraction layer. No accounts,
no saved worksheets, no scoring, and no server.
