namespace HomeworkTemplates.Core.Tests;

public class WorksheetSpecTests
{
    private static Dictionary<string, string?> Query(params (string Key, string? Value)[] pairs) =>
        pairs.ToDictionary(pair => pair.Key, pair => pair.Value);

    [Fact]
    public void Rejects_a_table_outside_the_factor_range()
    {
        var ok = WorksheetSpec.TryParse(Query(("tables", "25")), out _, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public void Rejects_a_multiplier_range_whose_min_exceeds_its_max()
    {
        var ok = WorksheetSpec.TryParse(Query(("min", "10"), ("max", "3")), out _, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public void Reads_all_as_a_null_problem_count()
    {
        var ok = WorksheetSpec.TryParse(Query(("count", "all")), out var spec, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.Null(spec.ProblemCount);
    }

    [Fact]
    public void Rejects_a_problem_count_below_one()
    {
        var ok = WorksheetSpec.TryParse(Query(("count", "0")), out _, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public void Clamps_a_problem_count_above_what_the_tables_can_produce()
    {
        // tables 2,3 x multipliers 1..5 is 10 distinct problems.
        var ok = WorksheetSpec.TryParse(
            Query(("tables", "2,3"), ("min", "1"), ("max", "5"), ("count", "999")),
            out var spec, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal(10, spec.ProblemCount);
    }

    [Fact]
    public void An_all_spec_round_trips_through_its_query_string()
    {
        var all = WorksheetSpec.Default with { ProblemCount = null };
        var query = all.ToQuery().ToDictionary(pair => pair.Key, pair => (string?)pair.Value);

        Assert.True(WorksheetSpec.TryParse(query, out var spec, out _));
        Assert.Null(spec.ProblemCount);
    }

    [Fact]
    public void Defaults_to_showing_all_problems()
    {
        Assert.Null(WorksheetSpec.Default.ProblemCount);
    }

    [Fact]
    public void Falls_back_to_the_default_spec_when_keys_are_missing()
    {
        var ok = WorksheetSpec.TryParse(Query(), out var spec, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal(WorksheetSpec.Default, spec);
    }

    [Fact]
    public void Rejects_an_unknown_order_value()
    {
        var ok = WorksheetSpec.TryParse(Query(("order", "diagonal")), out _, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public void Rejects_an_unknown_orientation_value()
    {
        var ok = WorksheetSpec.TryParse(Query(("orient", "diagonal")), out _, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public void Orientation_survives_a_query_round_trip()
    {
        var portrait = WorksheetSpec.Default with { Orientation = PageOrientation.Portrait };
        var query = portrait.ToQuery().ToDictionary(pair => pair.Key, pair => (string?)pair.Value);

        Assert.True(WorksheetSpec.TryParse(query, out var spec, out _));
        Assert.Equal(PageOrientation.Portrait, spec.Orientation);
    }

    [Fact]
    public void Rejects_an_unknown_paper_size()
    {
        var ok = WorksheetSpec.TryParse(Query(("paper", "foolscap")), out _, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
    }

    [Fact]
    public void Defaults_to_letter_paper()
    {
        Assert.Equal(PaperSize.Letter, WorksheetSpec.Default.Paper);
    }

    [Fact]
    public void Defaults_to_sequential_order()
    {
        Assert.Equal(ProblemOrder.Sequential, WorksheetSpec.Default.Order);
    }

    [Fact]
    public void Defaults_to_landscape_orientation()
    {
        Assert.Equal(PageOrientation.Landscape, WorksheetSpec.Default.Orientation);
    }

    [Fact]
    public void Defaults_to_a_multiplier_max_of_ten()
    {
        Assert.Equal(10, WorksheetSpec.Default.MultiplierMax);
    }

    [Fact]
    public void Defaults_to_tables_two_through_eleven()
    {
        Assert.Equal(new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, WorksheetSpec.Default.Tables);
    }

    [Fact]
    public void Paper_size_survives_a_query_round_trip()
    {
        var legal = WorksheetSpec.Default with { Paper = PaperSize.Legal };
        var query = legal.ToQuery().ToDictionary(pair => pair.Key, pair => (string?)pair.Value);

        Assert.True(WorksheetSpec.TryParse(query, out var spec, out _));
        Assert.Equal(PaperSize.Legal, spec.Paper);
    }

    [Fact]
    public void Defaults_to_grid_lines_off()
    {
        Assert.False(WorksheetSpec.Default.ShowGridLines);
    }

    [Fact]
    public void Grid_lines_flag_survives_a_query_round_trip()
    {
        var ruled = WorksheetSpec.Default with { ShowGridLines = true };
        var query = ruled.ToQuery().ToDictionary(pair => pair.Key, pair => (string?)pair.Value);

        Assert.True(WorksheetSpec.TryParse(query, out var spec, out _));
        Assert.True(spec.ShowGridLines);
    }

    [Fact]
    public void Rejects_a_non_boolean_grid_value()
    {
        var ok = WorksheetSpec.TryParse(Query(("grid", "yes")), out _, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
    }
}
