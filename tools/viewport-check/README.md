# viewport-check

Playwright script that verifies the worksheet preview at mobile/tablet/desktop
viewport widths, across every paper size and orientation. Guards two regressions:
the preview overflowing its container (issue #8), and grid-line borders
disappearing at fractional device pixel ratios (issue #24).

Not a test framework — one script, run manually or in CI before a preview-affecting
change ships.

## Run

```
npm install
npx playwright install chromium
npm run check
```

The script starts `dotnet run --project src/HomeworkTemplates.Web` itself, waits
for it to come up, runs the checks, and shuts it down. Screenshots land in
`screenshots/` (gitignored) for eyeball review; a non-zero exit code means a check
failed, with details printed to stderr.
