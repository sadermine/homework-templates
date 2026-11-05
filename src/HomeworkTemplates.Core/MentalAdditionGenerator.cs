namespace HomeworkTemplates.Core;

public sealed record AdditionProblem(int Left, int Right)
{
    public int Answer => Left + Right;
}

public static class MentalAdditionGenerator
{
    public static AdditionProblem Example { get; } = new(43, 24);
    public const int PracticeCount = 14;

    // Include single-digit addends and 100, as in the reference worksheet.
    public static IReadOnlyList<AdditionProblem> Generate(int seed)
    {
        var random = new Random(seed);
        var used = new HashSet<(int, int)>
        {
            (Example.Right, Example.Left),
        };
        var problems = new List<AdditionProblem>();
        while (problems.Count < PracticeCount)
        {
            var left = random.Next(10, 101);
            var right = random.Next(1, 101);
            if (used.Add((Math.Min(left, right), Math.Max(left, right))))
            {
                problems.Add(new AdditionProblem(left, right));
            }
        }
        return problems;
    }
}
