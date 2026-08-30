# Homework Templates

Printable homework worksheets generated in the browser. Pick a template, set the
options, and print. Currently ships one template: multiplication-table practice
with an optional answer key.

Live: https://sadermine.github.io/homework-templates/

## Run locally

```
dotnet run --project src/HomeworkTemplates.Web
```

## Test

```
dotnet test
```

## Layout

| Path | Contents |
| --- | --- |
| `src/HomeworkTemplates.Core` | `WorksheetSpec`, `WorksheetGenerator`, `TemplateCatalog` — pure, no UI |
| `src/HomeworkTemplates.Web` | Blazor WebAssembly app: pages, sheet components, print CSS |
| `tests/HomeworkTemplates.Core.Tests` | xUnit tests for the generator and spec parsing |
| `.github/workflows/deploy.yml` | Publishes to GitHub Pages on push to `main` |

## Adding a template

Add a `TemplateInfo` entry to `TemplateCatalog.All` and a matching `@page` under
`src/HomeworkTemplates.Web/Pages`.
