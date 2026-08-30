namespace HomeworkTemplates.Core;

/// <summary>
/// Permutes a problem list so a row-major CSS grid renders it column-major:
/// reading order runs down each column, then across to the next.
/// </summary>
public static class ProblemLayout
{
    public static IReadOnlyList<Problem> ColumnMajor(IReadOnlyList<Problem> problems, int columns)
    {
        if (columns <= 1 || problems.Count <= columns)
        {
            return problems;
        }

        var rows = (problems.Count + columns - 1) / columns;

        // The first (Count % columns) columns take one extra problem, so every empty
        // cell lands at the end of the last row: the tail of the row-major sequence.
        var tall = problems.Count % columns;
        var heights = new int[columns];
        var starts = new int[columns];
        for (int c = 0, next = 0; c < columns; c++)
        {
            heights[c] = rows - (tall == 0 || c < tall ? 0 : 1);
            starts[c] = next;
            next += heights[c];
        }

        var placed = new List<Problem>(problems.Count);
        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < columns; c++)
            {
                if (r < heights[c])
                {
                    placed.Add(problems[starts[c] + r]);
                }
            }
        }

        return placed;
    }
}
