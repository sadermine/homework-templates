namespace HomeworkTemplates.Core;

public sealed record TemplateInfo(string Slug, string Name, string Description);

public static class TemplateCatalog
{
    public static IReadOnlyList<TemplateInfo> All { get; } = new[]
    {
        new TemplateInfo(
            "multiplication",
            "Multiplication Tables",
            "Rows of times-table problems to solve, with an optional answer key."),
        new TemplateInfo(
            "addition-mental-math",
            "Addition (Mental Math)",
            "Practice making ten and breaking addition into tens and ones, with a worked example."),
    };
}
