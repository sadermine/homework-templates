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

/// <summary>One printed page of a worksheet. <paramref name="Number"/> is 1-based.</summary>
public sealed record WorksheetPage(int Number, IReadOnlyList<Problem> Problems);

public sealed record Worksheet(WorksheetSpec Spec, IReadOnlyList<WorksheetPage> Pages)
{
    /// <summary>Every problem across every page, in print order.</summary>
    public IEnumerable<Problem> Problems => Pages.SelectMany(page => page.Problems);
}
