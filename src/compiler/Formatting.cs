using System.Text;

namespace Noa.Compiler;

internal static class Formatting
{
    /// <summary>
    /// Joins a collection of values into a string with "or" and an oxford comma.
    /// </summary>
    /// <param name="values">The values to join.</param>
    /// <typeparam name="T">The type of the values to join.</typeparam>
    public static string? JoinOxfordOr<T>(IEnumerable<T> values) =>
        JoinWithSeparators(", ", ", or ", " or ", values);
    
    /// <summary>
    /// Joins a collection of values into a string using an individual separator
    /// between two values, between the two ending values,
    /// and between the single two values of the sequence if it only contains two values.
    /// </summary>
    /// <param name="separator">The separator to use between values before the ending pair.</param>
    /// <param name="endingSeparator">The separator to using between the ending pair.</param>
    /// <param name="dualSeparator">The separator to use between if the sequence of contains two values.</param>
    /// <param name="values">The values to join.</param>
    /// <typeparam name="T">The type of the values to join.</typeparam>
    public static string? JoinWithSeparators<T>(
        string separator,
        string endingSeparator,
        string dualSeparator,
        IEnumerable<T> values)
    {
        using var enumerator = values.GetEnumerator();

        if (!enumerator.MoveNext()) return null;
        var first = enumerator.Current;

        if (!enumerator.MoveNext()) return first?.ToString();
        var second = enumerator.Current;

        var builder = new StringBuilder(first?.ToString());
        
        var count = 2;
        var previous = second;
        while (enumerator.MoveNext())
        {
            builder.Append(separator).Append(previous);
            
            count++;
            previous = enumerator.Current;
        }

        if (count == 2) builder.Append(dualSeparator).Append(previous);
        else if (count > 2) builder.Append(endingSeparator).Append(previous);

        return builder.ToString();
    } 
}
