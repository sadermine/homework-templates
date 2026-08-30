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

    [Theory]
    [MemberData(nameof(ColumnTable))]
    public void Column_count_matches_the_agreed_table(PaperSize paper, PageOrientation orientation, int expected)
    {
        Assert.Equal(expected, PageLayout.Measure(paper, orientation).Columns);
    }

    [Theory]
    [MemberData(nameof(ColumnTable))]
    public void Every_column_is_wide_enough_for_a_problem(PaperSize paper, PageOrientation orientation, int _)
    {
        var page = PageLayout.Measure(paper, orientation);

        Assert.True(
            page.ColumnWidthMm >= PageLayout.MinProblemMm,
            $"{paper} {orientation}: {page.ColumnWidthMm:0.0}mm columns, need {PageLayout.MinProblemMm:0.0}mm.");
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
            }
        }
    }
}
