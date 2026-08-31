using HomeworkTemplates.Core;

namespace HomeworkTemplates.Core.Tests;

public class PageLayoutTests
{
    public static TheoryData<PaperSize, PageOrientation, int> ColumnTable => new()
    {
        { PaperSize.Letter, PageOrientation.Portrait, 3 },
        { PaperSize.Letter, PageOrientation.Landscape, 5 },
        { PaperSize.Legal, PageOrientation.Portrait, 3 },
        { PaperSize.Legal, PageOrientation.Landscape, 6 },
        { PaperSize.Tabloid, PageOrientation.Portrait, 4 },
        { PaperSize.Tabloid, PageOrientation.Landscape, 8 },
        { PaperSize.A4, PageOrientation.Portrait, 3 },
        { PaperSize.A4, PageOrientation.Landscape, 5 },
        { PaperSize.A5, PageOrientation.Portrait, 2 },
        { PaperSize.A5, PageOrientation.Landscape, 3 },
    };

    public static TheoryData<PaperSize, PageOrientation, int> RowTable => new()
    {
        { PaperSize.Letter, PageOrientation.Portrait, 14 },
        { PaperSize.Letter, PageOrientation.Landscape, 10 },
        { PaperSize.Legal, PageOrientation.Portrait, 19 },
        { PaperSize.Legal, PageOrientation.Landscape, 10 },
        { PaperSize.Tabloid, PageOrientation.Portrait, 25 },
        { PaperSize.Tabloid, PageOrientation.Landscape, 14 },
        { PaperSize.A4, PageOrientation.Portrait, 15 },
        { PaperSize.A4, PageOrientation.Landscape, 9 },
        { PaperSize.A5, PageOrientation.Portrait, 9 },
        { PaperSize.A5, PageOrientation.Landscape, 5 },
    };

    [Theory]
    [MemberData(nameof(ColumnTable))]
    public void Column_count_matches_the_agreed_table(PaperSize paper, PageOrientation orientation, int expected)
    {
        Assert.Equal(expected, PageLayout.Measure(paper, orientation).Columns);
    }

    [Theory]
    [MemberData(nameof(RowTable))]
    public void Row_count_matches_the_derived_table(PaperSize paper, PageOrientation orientation, int expected)
    {
        Assert.Equal(expected, PageLayout.Measure(paper, orientation).Rows);
    }

    [Fact]
    public void Letter_landscape_fits_exactly_ten_rows()
    {
        Assert.Equal(10, PageLayout.Measure(PaperSize.Letter, PageOrientation.Landscape).Rows);
    }

    [Theory]
    [MemberData(nameof(ColumnTable))]
    public void Every_column_is_wide_enough_for_a_problem(PaperSize paper, PageOrientation orientation, int _)
    {
        var page = PageLayout.Measure(paper, orientation);

        Assert.True(
            page.ColumnWidthMm >= PageLayout.MinProblemWidthMm,
            $"{paper} {orientation}: {page.ColumnWidthMm:0.0}mm columns, need {PageLayout.MinProblemWidthMm:0.0}mm.");
    }

    [Fact]
    public void Every_row_is_tall_enough_for_a_problem()
    {
        Assert.True(
            PageLayout.RowHeightMm >= PageLayout.MinProblemHeightMm,
            $"{PageLayout.RowHeightMm:0.0}mm row pitch, need {PageLayout.MinProblemHeightMm:0.0}mm.");
    }

    [Theory]
    [MemberData(nameof(RowTable))]
    public void Rows_and_chrome_fit_inside_the_page_height(PaperSize paper, PageOrientation orientation, int _)
    {
        var page = PageLayout.Measure(paper, orientation);
        var used = (page.Rows * PageLayout.RowHeightMm) + PageLayout.SheetChromeMm + (2 * PageLayout.MarginMm);

        Assert.True(
            used <= page.HeightMm + 1e-6,
            $"{paper} {orientation}: {page.Rows} rows use {used:0.0}mm of a {page.HeightMm:0.0}mm page.");
    }

    [Fact]
    public void Landscape_swaps_the_page_dimensions()
    {
        var portrait = PageLayout.Measure(PaperSize.Letter, PageOrientation.Portrait);
        var landscape = PageLayout.Measure(PaperSize.Letter, PageOrientation.Landscape);

        Assert.Equal(portrait.WidthMm, landscape.HeightMm);
        Assert.Equal(portrait.HeightMm, landscape.WidthMm);
        Assert.True(landscape.WidthMm > landscape.HeightMm);
    }

    [Theory]
    [InlineData(PaperSize.Letter, PageOrientation.Portrait, 42)]
    [InlineData(PaperSize.Letter, PageOrientation.Landscape, 50)]
    [InlineData(PaperSize.A5, PageOrientation.Portrait, 18)]
    public void Problems_per_page_is_columns_times_rows(PaperSize paper, PageOrientation orientation, int expected)
    {
        var page = PageLayout.Measure(paper, orientation);

        Assert.Equal(page.Columns * page.Rows, page.ProblemsPerPage);
        Assert.Equal(expected, page.ProblemsPerPage);
    }

    [Fact]
    public void Every_paper_size_has_geometry_in_both_orientations()
    {
        foreach (var paper in Enum.GetValues<PaperSize>())
        {
            foreach (var orientation in Enum.GetValues<PageOrientation>())
            {
                var page = PageLayout.Measure(paper, orientation);

                Assert.True(page.WidthMm > 0);
                Assert.True(page.HeightMm > 0);
                Assert.True(page.Columns > 0);
                Assert.True(page.Rows > 0);
            }
        }
    }
}
