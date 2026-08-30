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

        var problems = new List<Problem>(spec.ProblemCount);

        // A spec built directly (not via TryParse) can carry no tables or an inverted
        // multiplier range, leaving nothing to draw from.
        if (pool.Count == 0)
        {
            return new Worksheet(spec, problems);
        }

        void Arrange()
        {
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
        }

        while (problems.Count < spec.ProblemCount)
        {
            Arrange();
            if (problems.Count > 0 && pool.Count > 1 && pool[0] == problems[^1])
            {
                (pool[0], pool[1]) = (pool[1], pool[0]);
            }

            foreach (var problem in pool)
            {
                if (problems.Count == spec.ProblemCount)
                {
                    break;
                }

                problems.Add(problem);
            }
        }

        var ordered = spec.Order == ProblemOrder.Sequential
            ? ProblemLayout.ColumnMajor(problems, spec.Page.Columns)
            : problems;

        return new Worksheet(spec, ordered);
    }
}
