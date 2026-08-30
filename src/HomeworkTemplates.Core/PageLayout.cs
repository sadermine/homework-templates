namespace HomeworkTemplates.Core;

/// <summary>Printed-page geometry for one paper size and orientation.</summary>
public readonly record struct PageMetrics(
    double WidthMm,
    double HeightMm,
    int Columns,
    double ColumnWidthMm);

public static class PageLayout
{
    public const double MarginMm = 14;
    public const double ColumnGapMm = 8;

    /// <summary>Width a "12 &#215; 12 = ___" cell needs: 9em at the sheet's 1.1rem, at 96dpi.</summary>
    public const double MinProblemMm = 9 * 1.1 * 16 * (25.4 / 96);

    /// <summary>
    /// Column counts are authored per paper and orientation, not derived. No single
    /// width formula yields Letter's specified 3 portrait / 5 landscape. The other
    /// sizes hold Letter's implied column pitch. <see cref="PageMetrics.ColumnWidthMm"/>
    /// is returned so a test can assert every column still fits a problem. A new
    /// <see cref="PaperSize"/> without an arm here fails PageLayoutTests, not the build.
    /// </summary>
    public static PageMetrics Measure(PaperSize paper, PageOrientation orientation)
    {
        var (portraitWidth, portraitHeight, portraitColumns, landscapeColumns) = paper switch
        {
            PaperSize.Letter => (215.9, 279.4, 3, 5),
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

        return new PageMetrics(width, height, columns, columnWidth);
    }
}
