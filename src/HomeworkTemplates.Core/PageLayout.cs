namespace HomeworkTemplates.Core;

/// <summary>Printed-page geometry for one paper size and orientation.</summary>
public readonly record struct PageMetrics(
    double WidthMm,
    double HeightMm,
    int Columns,
    double ColumnWidthMm,
    int Rows);

public static class PageLayout
{
    public const double MarginMm = 14;
    public const double ColumnGapMm = 8;

    private const double MmPerPx = 25.4 / 96;

    /// <summary>Width a "12 &#215; 12 = ___" cell needs: 9em at the sheet's 1.1rem, at 96dpi.</summary>
    public const double MinProblemWidthMm = 9 * 1.1 * 16 * MmPerPx;

    /// <summary>Height one problem line occupies: 1.1rem at the body line-height of 1.5, at 96dpi.</summary>
    public const double MinProblemHeightMm = 1.1 * 1.5 * 16 * MmPerPx;

    /// <summary>
    /// Vertical space the sheet header and footer take from the page, mirrored from
    /// <c>wwwroot/css/print.css</c>. Drift here changes printed output with nothing failing,
    /// so the CSS rules carry a comment pointing back to this constant. The budget assumes a
    /// single-line title; an 80-character title wraps on narrow paper and steals rows, which
    /// is left for issue #9's pagination to absorb.
    /// </summary>
    public const double SheetChromeMm =
        1.5 * 1.2 * 16 * MmPerPx   // .sheet-title line: 1.5rem at heading line-height 1.2
        + 5                        // .sheet-title margin-bottom
        + 1.0 * 1.5 * 16 * MmPerPx // .name-date line: 1rem at body line-height 1.5
        + 10                       // .sheet-header margin-bottom
        + 12                       // .sheet-footer margin-top
        + 0.75 * 1.5 * 16 * MmPerPx; // .sheet-footer line: 0.75rem at body line-height 1.5

    private const double LetterShortEdgeMm = 215.9;
    private const int AnchorRows = 10;

    /// <summary>
    /// Row pitch every sheet prints at, so a full page is a known row count. Anchored to
    /// issue #13: "For letter landscape, I want there to be 10 rows per page. Use the margins
    /// calculated from letter landscape to calculate all the other combinations."
    /// </summary>
    public const double RowHeightMm =
        (LetterShortEdgeMm - (2 * MarginMm) - SheetChromeMm) / AnchorRows;

    // SheetChromeMm is a sum of six terms; re-association can shift its last bit and turn the
    // anchor's exact 10.0 into a floored 9. This epsilon protects the anchor and moves no
    // other combination (the nearest, A4 landscape, has 0.58 of a row to spare).
    private const double RowFitTolerance = 1e-9;

    /// <summary>
    /// Column counts are authored per paper and orientation, not derived. No single
    /// width formula yields Letter's specified 3 portrait / 5 landscape. The other
    /// sizes hold Letter's implied column pitch. Row counts, by contrast, are derived
    /// from the Letter-landscape anchor in <see cref="RowHeightMm"/>.
    /// <see cref="PageMetrics.ColumnWidthMm"/> is returned so a test can assert every
    /// column still fits a problem. A new <see cref="PaperSize"/> without an arm here
    /// fails PageLayoutTests, not the build.
    /// </summary>
    public static PageMetrics Measure(PaperSize paper, PageOrientation orientation)
    {
        var (portraitWidth, portraitHeight, portraitColumns, landscapeColumns) = paper switch
        {
            PaperSize.Letter => (LetterShortEdgeMm, 279.4, 3, 5),
            PaperSize.Legal => (215.9, 355.6, 3, 6),
            PaperSize.Tabloid => (279.4, 431.8, 4, 8),
            PaperSize.A4 => (210.0, 297.0, 3, 5),
            PaperSize.A5 => (148.0, 210.0, 2, 3),
            _ => throw new ArgumentOutOfRangeException(nameof(paper), paper, "No page geometry for this paper size."),
        };

        var landscape = orientation == PageOrientation.Landscape;
        var width = landscape ? portraitHeight : portraitWidth;
        var height = landscape ? portraitWidth : portraitHeight;
        var columns = landscape ? landscapeColumns : portraitColumns;

        var usable = width - (2 * MarginMm);
        var columnWidth = (usable - (ColumnGapMm * (columns - 1))) / columns;

        var gridHeight = height - (2 * MarginMm) - SheetChromeMm;
        var rows = Math.Max(1, (int)Math.Floor((gridHeight / RowHeightMm) + RowFitTolerance));

        return new PageMetrics(width, height, columns, columnWidth, rows);
    }
}
