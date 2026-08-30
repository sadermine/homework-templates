using HomeworkTemplates.Core;

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
    public void Rejects_a_problem_count_above_the_maximum()
    {
        var ok = WorksheetSpec.TryParse(
            Query(("count", (WorksheetSpec.MaxProblemCount + 1).ToString())), out _, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
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
        var landscape = WorksheetSpec.Default with { Orientation = PageOrientation.Landscape };
        var query = landscape.ToQuery().ToDictionary(pair => pair.Key, pair => (string?)pair.Value);

        Assert.True(WorksheetSpec.TryParse(query, out var spec, out _));
        Assert.Equal(PageOrientation.Landscape, spec.Orientation);
    }
}
