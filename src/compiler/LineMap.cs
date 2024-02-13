namespace Noa.Compiler;

/// <summary>
/// A mapping of lines and character positions of a text.
/// </summary>
public sealed class LineMap
{
    /// <summary>
    /// Creates a new line map.
    /// </summary>
    /// <param name="str">The string of character to create the map from.</param>
    public static LineMap Create(ReadOnlySpan<char> str)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the line with a specified <b>1-indexed</b> line number.
    /// </summary>
    /// <param name="lineNumber">The <b>1-indexed</b> line number of the line to get.</param>
    public Line GetLine(int lineNumber)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the character position at a specified position from the start of the text.
    /// </summary>
    /// <param name="position">The position in the text to get the character position at.</param>
    public CharacterPosition GetCharacterPosition(int position)
    {
        throw new NotImplementedException();
    }
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
