namespace HomeworkTemplates.Core;

public static class WorksheetGenerator
{
    public static Worksheet Generate(WorksheetSpec spec)
    {
        var random = new Random(spec.Seed);

        var pool = new List<Problem>();
        foreach (var table in spec.Tables)
        {
            for (var multiplier = spec.MultiplierMin; multiplier <= spec.MultiplierMax; multiplier++)
            {
                pool.Add(new Problem(table, multiplier));
            }
        }

        // A spec built directly (not via TryParse) can carry no tables or an inverted
        // multiplier range, leaving nothing to draw from.
        if (pool.Count == 0)
        {
            return new Worksheet(spec, [new WorksheetPage(1, [])]);
        }

        var target = Math.Min(spec.ProblemCount ?? pool.Count, pool.Count);

        if (spec.Order == ProblemOrder.Shuffled)
        {
            for (var i = pool.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
        }
        else
        {
            pool.Sort(static (a, b) => a.Left != b.Left ? a.Left.CompareTo(b.Left) : a.Right.CompareTo(b.Right));
        }

        var problems = pool.Take(target).ToList();

        var pages = problems
            .Chunk(spec.Page.ProblemsPerPage)
            .Select((chunk, index) => new WorksheetPage(
                index + 1,
                spec.Order == ProblemOrder.Sequential
                    ? ProblemLayout.ColumnMajor(chunk, spec.Page.Columns)
                    : chunk))
            .ToArray();

        return new Worksheet(spec, pages);
    }
}
