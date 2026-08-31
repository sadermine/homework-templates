using HomeworkTemplates.Core;

namespace HomeworkTemplates.Core.Tests;

public class WorksheetGeneratorTests
{
    private static WorksheetSpec Spec(
        int[]? tables = null,
        int min = 1,
        int max = 12,
        int? count = 30,
        ProblemOrder order = ProblemOrder.Shuffled,
        PaperSize paper = PaperSize.Letter,
        PageOrientation orientation = PageOrientation.Portrait,
        int seed = 1) =>
        new(
            tables ?? new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
            min,
            max,
            count,
            order,
            paper,
            orientation,
            seed,
            "Test",
            ShowNameAndDate: true,
            ShowGridLines: false);

    [Fact]
    public void Generate_returns_the_same_sequence_for_an_equal_spec()
    {
        var first = WorksheetGenerator.Generate(Spec());
        var second = WorksheetGenerator.Generate(Spec());

        Assert.Equal(first.Problems, second.Problems);
    }

    [Fact]
    public void Problem_count_matches_the_spec()
    {
        var worksheet = WorksheetGenerator.Generate(Spec(count: 30));

        Assert.Equal(30, worksheet.Problems.Count());
    }

    [Fact]
    public void Problem_count_clamps_to_the_pool_when_the_spec_asks_for_more()
    {
        var worksheet = WorksheetGenerator.Generate(Spec(tables: new[] { 2 }, min: 1, max: 3, count: 10));

        Assert.Equal(3, worksheet.Problems.Count());
    }

    [Fact]
    public void A_null_count_yields_every_distinct_problem_once()
    {
        var spec = Spec(
            tables: new[] { 2, 3, 4 }, min: 1, max: 12, count: null, order: ProblemOrder.Sequential);

        var worksheet = WorksheetGenerator.Generate(spec);

        var expected =
            from left in new[] { 2, 3, 4 }
            from right in Enumerable.Range(1, 12)
            select new Problem(left, right);

        Assert.Equal(36, worksheet.Problems.Count());
        Assert.Equal(expected, worksheet.Problems.OrderBy(p => p.Left).ThenBy(p => p.Right));
    }

    [Fact]
    public void Over_count_no_longer_repeats_a_table_across_pages()
    {
        // Issue #17: tables 2-10 x multipliers 1-10 is 90 distinct problems. A count of 100
        // used to refill the pool and print the 2s table a second time.
        var spec = Spec(
            tables: new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 }, min: 1, max: 10, count: 100,
            paper: PaperSize.Letter, orientation: PageOrientation.Landscape, order: ProblemOrder.Sequential);

        var worksheet = WorksheetGenerator.Generate(spec);

        Assert.Equal(new[] { 50, 40 }, worksheet.Pages.Select(page => page.Problems.Count));
        Assert.Equal(90, worksheet.Problems.Distinct().Count());
    }

    [Theory]
    [InlineData(PaperSize.Letter, PageOrientation.Portrait, 100, new[] { 42, 42, 16 })]
    [InlineData(PaperSize.A5, PageOrientation.Portrait, 30, new[] { 18, 12 })]
    public void Problems_split_into_pages_that_fill_before_they_spill(
        PaperSize paper, PageOrientation orientation, int count, int[] expectedSizes)
    {
        var worksheet = WorksheetGenerator.Generate(
            Spec(count: count, paper: paper, orientation: orientation, order: ProblemOrder.Sequential));

        Assert.Equal(expectedSizes, worksheet.Pages.Select(page => page.Problems.Count));
        Assert.Equal(Enumerable.Range(1, expectedSizes.Length), worksheet.Pages.Select(page => page.Number));
    }

    [Fact]
    public void Every_page_before_the_last_is_exactly_one_page_full()
    {
        var spec = Spec(count: 100, paper: PaperSize.Letter, orientation: PageOrientation.Portrait);
        var perPage = spec.Page.ProblemsPerPage;

        var worksheet = WorksheetGenerator.Generate(spec);

        Assert.All(worksheet.Pages.SkipLast(1), page => Assert.Equal(perPage, page.Problems.Count));
        Assert.True(worksheet.Pages[^1].Problems.Count <= perPage);
    }

    [Fact]
    public void Sequential_pages_carry_consecutive_slices_of_the_sorted_draw()
    {
        // Sorted draw is (2,1..12)(3,1..12)(4,1..6). A5 portrait holds 18 per page, so
        // page 2 is the next slice in reading order; the sheet grid flows it into columns.
        var spec = Spec(
            tables: new[] { 2, 3, 4 }, min: 1, max: 12, count: 30,
            paper: PaperSize.A5, orientation: PageOrientation.Portrait, order: ProblemOrder.Sequential);

        var worksheet = WorksheetGenerator.Generate(spec);

        Problem[] pageTwo =
        [
            new(3, 7), new(3, 8), new(3, 9), new(3, 10), new(3, 11), new(3, 12),
            new(4, 1), new(4, 2), new(4, 3), new(4, 4), new(4, 5), new(4, 6),
        ];
        Assert.Equal(pageTwo, worksheet.Pages[1].Problems);
    }

    [Fact]
    public void Pagination_neither_drops_nor_duplicates_a_problem()
    {
        var spec = Spec(
            tables: new[] { 2, 3, 4 }, min: 1, max: 12, count: 30,
            paper: PaperSize.A5, orientation: PageOrientation.Portrait, order: ProblemOrder.Sequential);

        var worksheet = WorksheetGenerator.Generate(spec);

        var sortedPool =
            from left in new[] { 2, 3, 4 }
            from right in Enumerable.Range(1, 12)
            select new Problem(left, right);

        Assert.Equal(
            sortedPool.Take(30),
            worksheet.Problems.OrderBy(p => p.Left).ThenBy(p => p.Right));
    }

    [Fact]
    public void Every_left_factor_is_one_of_the_selected_tables()
    {
        var tables = new[] { 3, 7, 11 };

        var worksheet = WorksheetGenerator.Generate(Spec(tables: tables, count: 50));

        Assert.All(worksheet.Problems, p => Assert.Contains(p.Left, tables));
    }

    [Fact]
    public void Every_right_factor_is_within_the_multiplier_range()
    {
        var worksheet = WorksheetGenerator.Generate(Spec(min: 4, max: 9, count: 50));

        Assert.All(worksheet.Problems, p =>
        {
            Assert.True(p.Right >= 4);
            Assert.True(p.Right <= 9);
        });
    }

    [Fact]
    public void Answer_is_the_product_of_both_factors()
    {
        var worksheet = WorksheetGenerator.Generate(Spec(count: 50));

        Assert.All(worksheet.Problems, p => Assert.Equal(p.Left * p.Right, p.Answer));
    }

    [Fact]
    public void Sequential_orders_problems_by_table_then_multiplier()
    {
        var spec = Spec(tables: new[] { 3, 2 }, min: 2, max: 4, count: 6, order: ProblemOrder.Sequential);

        var worksheet = WorksheetGenerator.Generate(spec);

        Problem[] expected =
        [
            new(2, 2), new(2, 3), new(2, 4),
            new(3, 2), new(3, 3), new(3, 4),
        ];
        Assert.Equal(expected, worksheet.Problems);
    }

    [Fact]
    public void Shuffled_and_sequential_differ_in_order()
    {
        var spec = Spec(tables: new[] { 3, 2 }, min: 2, max: 4, count: 6, order: ProblemOrder.Shuffled);

        var shuffled = WorksheetGenerator.Generate(spec).Problems;
        var sequential = WorksheetGenerator.Generate(spec with { Order = ProblemOrder.Sequential }).Problems;

        Assert.NotEqual(sequential, shuffled);
    }

    [Fact]
    public void Shuffled_yields_different_sequences_for_different_seeds()
    {
        var one = WorksheetGenerator.Generate(Spec(order: ProblemOrder.Shuffled, seed: 1));
        var two = WorksheetGenerator.Generate(Spec(order: ProblemOrder.Shuffled, seed: 2));

        Assert.NotEqual(one.Problems, two.Problems);
    }

    [Fact]
    public void The_sheet_and_the_answer_key_draw_identical_problems()
    {
        var spec = Spec(seed: 42);

        var sheet = WorksheetGenerator.Generate(spec);
        var key = WorksheetGenerator.Generate(spec);

        Assert.Equal(sheet.Problems, key.Problems);
    }

    [Fact]
    public void Default_spec_round_trips_through_its_query_string()
    {
        var query = WorksheetSpec.Default.ToQuery()
            .ToDictionary(pair => pair.Key, pair => (string?)pair.Value);

        Assert.True(WorksheetSpec.TryParse(query, out var spec, out var error));
        Assert.Null(error);
        Assert.Equal(WorksheetSpec.Default.Tables, spec.Tables);
        Assert.Equal(WorksheetSpec.Default with { Tables = spec.Tables }, spec);
    }
}
