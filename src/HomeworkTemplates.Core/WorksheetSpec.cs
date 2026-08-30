using System.Globalization;

namespace HomeworkTemplates.Core;

public sealed record WorksheetSpec(
    IReadOnlyList<int> Tables,
    int MultiplierMin,
    int MultiplierMax,
    int ProblemCount,
    ProblemOrder Order,
    PageOrientation Orientation,
    int Seed,
    string Title,
    bool ShowNameAndDate)
{
    public const int MinFactor = 1;
    public const int MaxFactor = 20;
    public const int MinProblemCount = 1;
    public const int MaxProblemCount = 100;
    public const int MaxTitleLength = 80;

    public static WorksheetSpec Default { get; } = new(
        Tables: new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
        MultiplierMin: 1,
        MultiplierMax: 12,
        ProblemCount: 30,
        Order: ProblemOrder.Shuffled,
        Orientation: PageOrientation.Portrait,
        Seed: 1,
        Title: "Multiplication Practice",
        ShowNameAndDate: true);

    /// <summary>
    /// Parses a spec from untrusted string values, typically a URL query string.
    /// Returns false and a human-readable message on any invalid or out-of-range input.
    /// Missing keys fall back to <see cref="Default"/>; present-but-invalid keys are rejected.
    /// </summary>
    public static bool TryParse(
        IReadOnlyDictionary<string, string?> values,
        out WorksheetSpec spec,
        out string? error)
    {
        spec = Default;
        error = null;

        var tables = Default.Tables;
        if (values.TryGetValue("tables", out var tablesRaw) && !string.IsNullOrWhiteSpace(tablesRaw))
        {
            var parsed = new List<int>();
            foreach (var part in tablesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                {
                    error = $"'{part}' is not a whole number.";
                    return false;
                }

                if (n < MinFactor || n > MaxFactor)
                {
                    error = $"Table {n} is outside {MinFactor}-{MaxFactor}.";
                    return false;
                }

                if (!parsed.Contains(n))
                {
                    parsed.Add(n);
                }
            }

            if (parsed.Count == 0)
            {
                error = "Pick at least one table.";
                return false;
            }

            tables = parsed;
        }

        if (!TryReadInt(values, "min", Default.MultiplierMin, out var min, out error))
        {
            return false;
        }

        if (!TryReadInt(values, "max", Default.MultiplierMax, out var max, out error))
        {
            return false;
        }

        if (min < MinFactor || max > MaxFactor || min > max)
        {
            error = $"Multiplier range must sit within {MinFactor}-{MaxFactor} and be non-empty.";
            return false;
        }

        if (!TryReadInt(values, "count", Default.ProblemCount, out var count, out error))
        {
            return false;
        }

        if (count < MinProblemCount || count > MaxProblemCount)
        {
            error = $"Problem count must be {MinProblemCount}-{MaxProblemCount}.";
            return false;
        }

        var order = Default.Order;
        if (values.TryGetValue("order", out var orderRaw) && !string.IsNullOrWhiteSpace(orderRaw))
        {
            if (!Enum.TryParse(orderRaw, ignoreCase: true, out order))
            {
                error = $"Unknown order '{orderRaw}'.";
                return false;
            }
        }

        var orientation = Default.Orientation;
        if (values.TryGetValue("orient", out var orientRaw) && !string.IsNullOrWhiteSpace(orientRaw))
        {
            if (!Enum.TryParse(orientRaw, ignoreCase: true, out orientation))
            {
                error = $"Unknown orientation '{orientRaw}'.";
                return false;
            }
        }

        if (!TryReadInt(values, "seed", Default.Seed, out var seed, out error))
        {
            return false;
        }

        var title = Default.Title;
        if (values.TryGetValue("title", out var titleRaw) && titleRaw is not null)
        {
            title = titleRaw.Trim();
            if (title.Length == 0)
            {
                title = Default.Title;
            }
            else if (title.Length > MaxTitleLength)
            {
                error = $"Title must be {MaxTitleLength} characters or fewer.";
                return false;
            }
        }

        var showNameAndDate = Default.ShowNameAndDate;
        if (values.TryGetValue("names", out var namesRaw) && !string.IsNullOrWhiteSpace(namesRaw))
        {
            if (!bool.TryParse(namesRaw, out showNameAndDate))
            {
                error = $"'{namesRaw}' is not true or false.";
                return false;
            }
        }

        spec = new WorksheetSpec(tables, min, max, count, order, orientation, seed, title, showNameAndDate);
        return true;
    }

    /// <summary>Serializes the spec back to query-string pairs, the inverse of <see cref="TryParse"/>.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> ToQuery() => new[]
    {
        new KeyValuePair<string, string>("tables", string.Join(',', Tables)),
        new KeyValuePair<string, string>("min", MultiplierMin.ToString(CultureInfo.InvariantCulture)),
        new KeyValuePair<string, string>("max", MultiplierMax.ToString(CultureInfo.InvariantCulture)),
        new KeyValuePair<string, string>("count", ProblemCount.ToString(CultureInfo.InvariantCulture)),
        new KeyValuePair<string, string>("order", Order.ToString()),
        new KeyValuePair<string, string>("orient", Orientation.ToString()),
        new KeyValuePair<string, string>("seed", Seed.ToString(CultureInfo.InvariantCulture)),
        new KeyValuePair<string, string>("title", Title),
        new KeyValuePair<string, string>("names", ShowNameAndDate ? "true" : "false"),
    };

    private static bool TryReadInt(
        IReadOnlyDictionary<string, string?> values,
        string key,
        int fallback,
        out int result,
        out string? error)
    {
        error = null;
        result = fallback;
        if (!values.TryGetValue(key, out var raw) || string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
        {
            error = $"'{raw}' is not a whole number.";
            return false;
        }

        return true;
    }
}
