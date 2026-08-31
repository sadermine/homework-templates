using HomeworkTemplates.Core;

namespace HomeworkTemplates.Web;

/// <summary>
/// Renders <see cref="PageMetrics"/> as the CSS custom properties the sheet styles read.
/// Keeps the invariant number formatting in one place, out of the Razor markup.
/// </summary>
internal static class SheetStyle
{
    public static string For(PageMetrics page) => FormattableString.Invariant(
        $"--sheet-width: {page.WidthMm}mm; --page-margin: {PageLayout.MarginMm}mm; --cell-padding: {PageLayout.CellPaddingMm}mm; --row-height: {PageLayout.RowHeightMm:0.#####}mm");

    public static string GridTemplate(PageMetrics page) => FormattableString.Invariant(
        $"grid-template-columns: repeat({page.Columns}, minmax(0, 1fr)); grid-template-rows: repeat({page.Rows}, var(--row-height)); grid-auto-flow: column");
}
