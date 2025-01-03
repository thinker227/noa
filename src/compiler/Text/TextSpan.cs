namespace Noa.Compiler.Text;

/// <summary>
/// A span of characters within a text.
/// </summary>
/// <param name="Start">The start of the span from the start of the text.</param>
/// <param name="End">The end of the span from the start of the text.</param>
public readonly record struct TextSpan(int Start, int End)
{
    /// <summary>
    /// The length of the span within the text.
    /// </summary>
    public int Length => End - Start;

    /// <summary>
    /// Creates a new span from a start position and a length.
    /// </summary>
    /// <param name="start">The start of the span from the start of the text.</param>
    /// <param name="length">The length of the span within the text.</param>
    public static TextSpan FromLength(int start, int length) =>
        new(start, start + length);

    /// <summary>
    /// Creates a new span from the total span of two other spans.
    /// The spans do not have to be connected or next to each other.
    /// </summary>
    /// <param name="a">The first span.</param>
    /// <param name="b">The second span.</param>
    /// <returns>
    /// The span between <paramref name="a"/> and <paramref name="b"/>.
    /// </returns>
    public static TextSpan Between(TextSpan a, TextSpan b) =>
        new(int.Min(a.Start, b.Start), int.Max(a.End, b.End));

    /// <inheritdoc cref="Between(TextSpan, TextSpan)"/>
    /// <returns>
    /// The span between <paramref name="a"/> and <paramref name="b"/>,
    /// or <paramref name="a"/> if <paramref name="b"/> is <see langword="null"/>.
    /// </returns>
    public static TextSpan Between(TextSpan a, TextSpan? b) =>
        b is { } bx ? Between(a, bx) : a;

    /// <summary>
    /// Return whether a position is within the span.
    /// </summary>
    /// <param name="position">The position to check.</param>
    public bool Contains(int position) =>
        position >= Start && position < End;

    public override string ToString() => $"{Start} to {End}";
}
