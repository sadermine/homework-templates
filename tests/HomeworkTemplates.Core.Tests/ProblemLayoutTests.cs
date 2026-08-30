using HomeworkTemplates.Core;

namespace HomeworkTemplates.Core.Tests;

public class ProblemLayoutTests
{
    private static IReadOnlyList<Problem> Sequence(int count) =>
        Enumerable.Range(0, count).Select(n => new Problem(1, n)).ToArray();

    private static int[] Indices(IReadOnlyList<Problem> problems) =>
        problems.Select(p => p.Right).ToArray();

    [Fact]
    public void Exact_multiple_reads_down_each_column()
    {
        var laid = ProblemLayout.ColumnMajor(Sequence(6), columns: 3);

        Assert.Equal(new[] { 0, 2, 4, 1, 3, 5 }, Indices(laid));
    }

    [Fact]
    public void Uneven_split_keeps_empty_cells_in_the_trailing_columns()
    {
        var laid = ProblemLayout.ColumnMajor(Sequence(9), columns: 4);

        Assert.Equal(new[] { 0, 3, 5, 7, 1, 4, 6, 8, 2 }, Indices(laid));
    }

    [Fact]
    public void Every_problem_survives_the_permutation_exactly_once()
    {
        var laid = ProblemLayout.ColumnMajor(Sequence(23), columns: 5);

        Assert.Equal(Enumerable.Range(0, 23), Indices(laid).OrderBy(n => n));
    }

    [Fact]
    public void A_single_column_is_returned_unchanged()
    {
        var input = Sequence(5);

        Assert.Same(input, ProblemLayout.ColumnMajor(input, columns: 1));
    }

    [Fact]
    public void Fewer_problems_than_columns_are_returned_unchanged()
    {
        var input = Sequence(3);

        Assert.Same(input, ProblemLayout.ColumnMajor(input, columns: 5));
    }
}
