export function printPage() {
    window.print();
}

export function copyText(text) {
    return navigator.clipboard.writeText(text);
}

// Scale-to-fit for the on-screen preview (issue #8). Each .sheet is authored at its
// true physical width (mm), which is wider than the preview container for anything
// past Letter portrait on a phone. When that happens, shrink it with a transform
// instead of leaving it to overflow.
//
// Scaling from "left top" rather than "center" means the shrunk box fills the
// container edge to edge by construction (scale = containerWidth / naturalWidth),
// with no separate centring step needed. transform doesn't affect layout flow, so
// the unscaled box still reserves its full height below the shrunk visual; the
// negative margin-bottom collapses that reserved space back out.
export function fitSheetsToContainer() {
    const container = document.querySelector(".page main .content");
    if (!container) {
        return;
    }

    const containerWidth = container.clientWidth;

    for (const sheet of document.querySelectorAll(".sheet")) {
        // Reset before measuring so a scale computed for a previous worksheet spec
        // (a different paper size, a hidden preview page) never leaks into this pass.
        sheet.style.transform = "";
        sheet.style.transformOrigin = "";
        sheet.style.marginLeft = "";
        sheet.style.marginRight = "";
        sheet.style.marginBottom = "";

        const naturalWidth = sheet.offsetWidth;
        if (naturalWidth === 0 || naturalWidth <= containerWidth) {
            // Not oversized (or hidden by the preview pager, which reports 0): leave
            // the CSS-declared 1:1 centred layout alone.
            continue;
        }

        const naturalHeight = sheet.offsetHeight;
        const baseMarginBottom = parseFloat(getComputedStyle(sheet).marginBottom) || 0;
        const scale = containerWidth / naturalWidth;

        sheet.style.transformOrigin = "left top";
        sheet.style.transform = `scale(${scale})`;
        sheet.style.marginLeft = "0";
        sheet.style.marginRight = "0";
        sheet.style.marginBottom = `${baseMarginBottom - naturalHeight * (1 - scale)}px`;
    }
}

let resizeTimer;
window.addEventListener("resize", () => {
    clearTimeout(resizeTimer);
    resizeTimer = setTimeout(fitSheetsToContainer, 150);
});
