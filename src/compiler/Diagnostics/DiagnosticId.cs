using System.Globalization;

namespace Noa.Compiler.Diagnostics;

public sealed class DiagnosticId : ISpanParsable<DiagnosticId>
{
    public string Major { get; }
    
    public string Category { get; }
    
    public int Numeric { get; }

    private DiagnosticId(string major, string category, int numeric)
    {
        Major = major;
        Category = category;
        Numeric = numeric;
    }

    public override string ToString() => $"{Major}-{Category}-{Numeric:D3}";

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out DiagnosticId result)
    {
        result = null!;

        var majorStart = 0;
        var majorLength = 0;
        for (; majorStart + majorLength < s.Length; majorLength++)
        {
            var c = s[majorLength];
            if (c == '-') break;
            if (!char.IsAsciiLetter(c)) return false;
        }

        if (majorLength == 0) return false;
        var major = s.Slice(majorStart, majorLength).ToString();

        var categoryStart = majorStart + majorLength + 1;
        var categoryLength = 0;
        for (; categoryStart + categoryLength < s.Length; categoryLength++)
        {
            var c = s[categoryStart + categoryLength];
            if (c == '-') break;
            if (!char.IsAsciiLetter(c)) return false;
        }

        if (categoryLength == 0) return false;
        var category = s.Slice(categoryStart, categoryLength).ToString();

        var numericStart = categoryStart + categoryLength + 1;
        if (numericStart >= s.Length) return false;
        if (!int.TryParse(s[numericStart..], NumberStyles.None, CultureInfo.InvariantCulture, out var numeric)) return false;

        result = new(major, category, numeric);
        return true;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out DiagnosticId result)
    {
        var span = s is not null
            ? s.AsSpan()
            : [];

        return TryParse(span, provider, out result);
    }

    public static DiagnosticId Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
        TryParse(s, provider, out var id)
            ? id
            : throw new FormatException($"Cannot parse '{s.ToString()}' into a diagnostic ID.");
    
    public static DiagnosticId Parse(string s, IFormatProvider? provider) =>
        TryParse(s, provider, out var id)
            ? id
            : throw new FormatException($"Cannot parse '{s}' into a diagnostic ID.");

    public static implicit operator DiagnosticId(string s) => Parse(s, null);
}
