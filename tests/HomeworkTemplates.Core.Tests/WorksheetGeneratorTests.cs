using HomeworkTemplates.Core;

namespace HomeworkTemplates.Core.Tests;

public class WorksheetGeneratorTests
{
    private static WorksheetSpec Spec(
        int[]? tables = null,
        int min = 1,
        int max = 12,
        int count = 30,
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
            ShowNameAndDate: true);

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

        Assert.Equal(30, worksheet.Problems.Count);
    }

    [Fact]
    public void Problem_count_matches_the_spec_when_it_exceeds_the_pool()
    {
        var worksheet = WorksheetGenerator.Generate(Spec(tables: new[] { 2 }, min: 1, max: 3, count: 10));

        Assert.Equal(10, worksheet.Problems.Count);
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
    public void Sequential_emits_the_pool_sorted_by_left_then_right()
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
