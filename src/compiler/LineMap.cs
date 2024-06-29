using System.Runtime.InteropServices;
using System.Collections;

namespace Noa.Compiler;

/// <summary>
/// A mapping between lines and character positions of a text.
/// </summary>
public sealed class LineMap : IReadOnlyList<Line>
{
    private readonly List<Line> lines;

    /// <summary>
    /// The total amount of lines in the map.
    /// </summary>
    public int LineCount => lines.Count;

    /// <summary>
    /// The size of the mapped text.
    /// </summary>
    public int Size => lines[^1].End;

    int IReadOnlyCollection<Line>.Count => LineCount;

    Line IReadOnlyList<Line>.this[int index] =>
        GetLine(index + 1);

    private LineMap(List<Line> lines) =>
        this.lines = lines;
    
    /// <summary>
    /// Creates a new line map.
    /// </summary>
    /// <param name="str">The string of characters to create the map from.</param>
    public static LineMap Create(ReadOnlySpan<char> str)
    {
        var lines = new List<Line>();

        var lineNumber = 1;
        var lineStart = 0;
        var lineLength = 0;
        
        for (var i = 0; i < str.Length; i++)
        {
            lineLength++;

            if (str[i] is not '\n') continue;
            
            lines.Add(new(lineNumber, lineStart, lineStart + lineLength));

            lineNumber++;
            lineStart = i + 1;
            lineLength = 0;
        }
        
        lines.Add(new(lineNumber, lineStart, lineStart + lineLength));

        return new(lines);
    }

    /// <summary>
    /// Gets the line with the specified <b>1-indexed</b> line number.
    /// </summary>
    /// <param name="lineNumber">The <b>1-indexed</b> line number of the line to get.</param>
    public Line GetLine(int lineNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lineNumber);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(lineNumber, lines.Count);

        return lines[lineNumber - 1];
    }

    /// <summary>
    /// Gets the character position at a specified position from the start of the text.
    /// </summary>
    /// <param name="position">The position in the text to get the character position at.</param>
    public CharacterPosition GetCharacterPosition(int position)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(position, lines[^1].End + 1);

        var span = CollectionsMarshal.AsSpan(lines);

        while (true)
        {
            Line line;
            
            if (span.Length == 0)
            {
                // If execution gets here then that means we're past the last line
                // and want to get the final character of the last line.
                line = lines[^1];
                return new(line, line.Length);
            }

            var index = span.Length / 2;
            line = span[index];

            if (position >= line.Start && position < line.End)
            {
                return new(line, position - line.Start);
            }

            span = position < line.Start
                ? span[..index]
                : span[(index + 1)..];
        }
    }

    /// <summary>
    /// Enumerates the lines of the map.
    /// </summary>
    public IEnumerator<Line> GetEnumerator() => lines.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// A representation of a line of text.
/// </summary>
/// <param name="LineNumber">The <b>1-indexed</b> line number of the line.</param>
/// <param name="Start">The character offset of the start of the line from the start of the text.</param>
/// <param name="End">The character offset of the end of the line from the start of the text.</param>
public readonly record struct Line(int LineNumber, int Start, int End)
{
    /// <summary>
    /// The length of the line in characters.
    /// </summary>
    public int Length => End - Start;
}

/// <summary>
/// A character position in a text.
/// </summary>
/// <param name="Line">The line of the character.</param>
/// <param name="Offset">The character's offset from the start of the line.</param>
public readonly record struct CharacterPosition(Line Line, int Offset);
