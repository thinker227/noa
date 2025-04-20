using System.Text.RegularExpressions;

namespace Noa.Compiler.Diagnostics;

/// <summary>
/// The ID of a diagnostic.
/// </summary>
/// <remarks>
/// A diagnostic ID consists of three parts -
/// a major name, a category, and a numeric ID.
/// When parsing an ID, the format should be provided as <c>MAJOR-CATEGORY-NUMERIC</c>,
/// for instance <c>NOA-OWO-621</c>.
/// </remarks>
public sealed partial record DiagnosticId : IParsable<DiagnosticId>
{
    /// <summary>
    /// The major name of the diagnostic.
    /// </summary>
    public string Major { get; }
    
    /// <summary>
    /// The category of the diagnostic.
    /// </summary>
    public string Category { get; }
    
    /// <summary>
    /// The numeric ID of the diagnostic.
    /// </summary>
    public int Numeric { get; }

    private DiagnosticId(string major, string category, int numeric)
    {
        Major = major;
        Category = category;
        Numeric = numeric;
    }

    public override string ToString() => $"{Major}-{Category}-{Numeric:D3}";

    [GeneratedRegex("^([A-Z]+)-([A-Z]+)-([0-9]+)$")]
    private static partial Regex IdRegex();

    public static bool TryParse(string? s, IFormatProvider? provider, out DiagnosticId result)
    {
        result = null!;

        if (s is null) return false;

        var match = IdRegex().Match(s);
        
        if (!match.Success) return false;

        var major = match.Groups[1].Value;
        var category = match.Groups[2].Value;

        var numericText = match.Groups[3].ValueSpan;
        if (!int.TryParse(numericText, out var numeric)) return false;

        result = new(major, category, numeric);
        return true;
    }
    
    public static DiagnosticId Parse(string s, IFormatProvider? provider) =>
        TryParse(s, provider, out var id)
            ? id
            : throw new FormatException($"Cannot parse '{s}' into a diagnostic ID.");

    /// <summary>
    /// Implicitly converts a string into a diagnostic ID.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    public static implicit operator DiagnosticId(string s) => Parse(s, null);
}
