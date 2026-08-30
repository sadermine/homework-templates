namespace HomeworkTemplates.Core;

public enum ProblemOrder
{
    Sequential,
    Shuffled,
}

public enum PageOrientation
{
    Portrait,
    Landscape,
}

public enum PaperSize
{
    Letter,
    Legal,
    Tabloid,
    A4,
    A5,
}

public readonly record struct Problem(int Left, int Right)
{
    public int Answer => Left * Right;
}

public sealed record Worksheet(WorksheetSpec Spec, IReadOnlyList<Problem> Problems);
