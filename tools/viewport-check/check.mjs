// Viewport verification for the mobile preview (issue #31).
//
// Starts the Blazor dev server, loads the multiplication worksheet across the
// paper/orientation matrix at three viewport widths, and asserts:
//   1. the sheet never renders wider than its container (regression guard for #8)
//   2. grid-line borders render with a non-zero width (regression guard for #24)
// plus two fixed-viewport cases at fractional device pixel ratios, since that's
// the leading hypothesis for why #24 happens at all.
//
// Usage: npm install && npx playwright install chromium && npm run check

import { chromium } from "playwright";
import { spawn } from "node:child_process";
import { setTimeout as delay } from "node:timers/promises";
import path from "node:path";
import fs from "node:fs";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, "..", "..");
const screenshotDir = path.join(__dirname, "screenshots");
fs.mkdirSync(screenshotDir, { recursive: true });

const PORT = 5179;
const BASE_URL = `http://127.0.0.1:${PORT}`;

const PAPERS = ["Letter", "Legal", "Tabloid", "A4", "A5"];
const ORIENTATIONS = ["Portrait", "Landscape"];
const VIEWPORTS = [
  { width: 390, height: 844, label: "mobile-390" },
  { width: 768, height: 1024, label: "tablet-768" },
  { width: 1280, height: 900, label: "desktop-1280" },
];
// Fractional DPRs seen on common Android devices, e.g. 412x915 @2.625x and
// 360x800 @2.75x, at a fixed paper size to keep the matrix small.
const FRACTIONAL_DPR_CASES = [
  { deviceScaleFactor: 2.625, width: 412, height: 915, label: "android-2.625x" },
  { deviceScaleFactor: 2.75, width: 360, height: 800, label: "android-2.75x" },
];

async function waitForServer(url, timeoutMs) {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    try {
      const res = await fetch(url);
      if (res.ok) {
        return;
      }
    } catch {
      // server not up yet
    }
    await delay(300);
  }
  throw new Error(`Server at ${url} did not respond within ${timeoutMs}ms`);
}

async function checkCase(context, { paper, orientation, viewportLabel }, failures) {
  const page = await context.newPage();
  const label = `${viewportLabel}_${paper}-${orientation}`;
  const url = `${BASE_URL}/multiplication?paper=${paper}&orient=${orientation}&grid=true`;

  await page.goto(url, { waitUntil: "networkidle" });
  await page.waitForSelector(".sheet", { timeout: 15_000 });

  // Two independent checks: documentElement.scrollWidth catches the whole page
  // scrolling (main{flex:1} needs min-width:0 or it grows to fit an oversized child
  // instead of clipping it). The sheet's own painted bounding box vs. .content's
  // clientWidth catches it not fitting inside the preview. Deliberately NOT
  // .content's scrollWidth: .content uses overflow-x:hidden, which crops the
  // *visual* overflow left over from transform:scale (transform doesn't shrink the
  // layout box) but scrollWidth still reports that full pre-clip layout size by
  // spec regardless — comparing scrollWidth to clientWidth here would flag every
  // scaled-down sheet as broken even though nothing is actually visible past the
  // container edge.
  const measurement = await page.evaluate(() => {
    const doc = document.documentElement;
    const container = document.querySelector(".page main .content");
    const sheet = document.querySelector(".sheet");
    const problem = document.querySelector(".problem-grid.ruled .problem");
    const style = problem ? getComputedStyle(problem) : null;
    return {
      viewportWidth: doc.clientWidth,
      pageScrollWidth: doc.scrollWidth,
      containerClientWidth: container?.clientWidth ?? null,
      sheetWidth: sheet?.getBoundingClientRect().width ?? null,
      borderWidth: style ? parseFloat(style.borderTopWidth) : null,
    };
  });

  await page.screenshot({ path: path.join(screenshotDir, `${label}.png`), fullPage: true });

  if (measurement.sheetWidth == null) {
    failures.push(`${label}: .sheet not found (page failed to render)`);
  } else if (measurement.pageScrollWidth > measurement.viewportWidth + 1) {
    failures.push(
      `${label}: page requires horizontal scroll (scrollWidth ${measurement.pageScrollWidth}px > viewport ${measurement.viewportWidth}px; sheet rendered at ${measurement.sheetWidth.toFixed(1)}px)`,
    );
  } else if (
    measurement.containerClientWidth != null &&
    measurement.sheetWidth > measurement.containerClientWidth + 1
  ) {
    failures.push(
      `${label}: sheet (${measurement.sheetWidth.toFixed(1)}px) does not fit the preview container (${measurement.containerClientWidth}px)`,
    );
  }

  if (measurement.borderWidth == null) {
    failures.push(`${label}: no .problem-grid.ruled .problem cell found to check grid-line borders`);
  } else if (measurement.borderWidth <= 0) {
    failures.push(`${label}: grid-line border rendered at ${measurement.borderWidth}px (expected > 0)`);
  }

  await page.close();
}

// Confirms the preview scaling never leaks into print: fitSheetsToContainer() leaves
// an inline transform/margin on a scaled sheet (see print.js), and only the
// !important rules in print.css's @media print block stand between that and a
// physically wrong printed page. Checked on the most-scaled case in the matrix
// (Tabloid landscape on a phone) since that's where a leak would be most visible.
async function checkPrintUnaffected(browser, failures) {
  const context = await browser.newContext({ viewport: { width: 390, height: 844 } });
  const page = await context.newPage();
  await page.goto(`${BASE_URL}/multiplication?paper=Tabloid&orient=Landscape`, {
    waitUntil: "networkidle",
  });
  await page.waitForSelector(".sheet", { timeout: 15_000 });
  await page.emulateMedia({ media: "print" });

  const printStyle = await page.evaluate(() => {
    const style = getComputedStyle(document.querySelector(".sheet"));
    return { transform: style.transform, marginLeft: style.marginLeft, marginBottom: style.marginBottom };
  });

  if (printStyle.transform !== "none") {
    failures.push(`print: .sheet keeps a preview transform under print media (${printStyle.transform})`);
  }
  if (printStyle.marginLeft !== "0px" || printStyle.marginBottom !== "0px") {
    failures.push(
      `print: .sheet keeps a preview margin under print media (left ${printStyle.marginLeft}, bottom ${printStyle.marginBottom})`,
    );
  }

  await context.close();
}

async function main() {
  console.log("Starting dotnet dev server...");
  const server = spawn(
    "dotnet",
    ["run", "--project", "src/HomeworkTemplates.Web", "--urls", BASE_URL, "--no-launch-profile"],
    { cwd: repoRoot, stdio: "pipe" },
  );
  let serverOutput = "";
  server.stdout.on("data", (d) => (serverOutput += d));
  server.stderr.on("data", (d) => (serverOutput += d));

  const failures = [];
  try {
    await waitForServer(BASE_URL, 60_000);
    console.log("Server is up. Running checks...");

    const browser = await chromium.launch();
    try {
      for (const viewport of VIEWPORTS) {
        const context = await browser.newContext({
          viewport: { width: viewport.width, height: viewport.height },
        });
        for (const paper of PAPERS) {
          for (const orientation of ORIENTATIONS) {
            await checkCase(context, { paper, orientation, viewportLabel: viewport.label }, failures);
          }
        }
        await context.close();
      }

      for (const dprCase of FRACTIONAL_DPR_CASES) {
        const context = await browser.newContext({
          viewport: { width: dprCase.width, height: dprCase.height },
          deviceScaleFactor: dprCase.deviceScaleFactor,
        });
        await checkCase(
          context,
          { paper: "Letter", orientation: "Landscape", viewportLabel: dprCase.label },
          failures,
        );
        await context.close();
      }

      await checkPrintUnaffected(browser, failures);
    } finally {
      await browser.close();
    }
  } finally {
    server.kill();
  }

  if (failures.length > 0) {
    console.error(`\n${failures.length} check(s) failed:\n`);
    for (const f of failures) {
      console.error(`  - ${f}`);
    }
    process.exitCode = 1;
    return;
  }

  console.log("\nAll viewport checks passed.");
}

main().catch((err) => {
  console.error(err);
  process.exitCode = 1;
});
