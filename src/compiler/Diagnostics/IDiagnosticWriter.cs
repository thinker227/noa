using Noa.Compiler.Symbols;

namespace Noa.Compiler.Diagnostics;

/// <summary>
/// Writes diagnostic messages.
/// </summary>
public interface IDiagnosticWriter
{
    /// <summary>
    /// Creates a new blank page to write a diagnostic onto.
    /// </summary>
    /// <param name="diagnostic">The diagnostic which will be written to the page.</param>
    /// <returns>A new blank diagnostic page.</returns>
    IDiagnosticPage CreatePage(IDiagnostic diagnostic);
}

/// <summary>
/// Writes diagnostic messages of a specific type.
/// </summary>
/// <typeparam name="T">The type of the messages the writer writes.</typeparam>
public interface IDiagnosticWriter<out T> : IDiagnosticWriter
{
    /// <inheritdoc cref="IDiagnosticWriter.CreatePage"/>
    new IDiagnosticPage<T> CreatePage(IDiagnostic diagnostic);

    IDiagnosticPage IDiagnosticWriter.CreatePage(IDiagnostic diagnostic) => CreatePage(diagnostic);
}

/// <summary>
/// A "page" which a diagnostic message can be written to.
/// </summary>
public interface IDiagnosticPage
{
    /// <summary>
    /// Writes a raw text string to the page.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <returns>The current page.</returns>
    IDiagnosticPage Raw(string text);

    /// <summary>
    /// Writes an emphasized text string to the page.
    /// </summary>
    /// <param name="text">The emphasized text to write.</param>
    /// <returns>The current page.</returns>
    IDiagnosticPage Emphasized(string text) => Raw(text);

    /// <summary>
    /// Writes a piece of source code without any special context to the page.
    /// </summary>
    /// <param name="source">The source code to write.</param>
    /// <returns>The current page.</returns>
    IDiagnosticPage Source(string source) => Raw(source);

    /// <summary>
    /// Writes a keyword to the page.
    /// </summary>
    /// <param name="keyword">The keyword to write.</param>
    /// <returns>The current page.</returns>
    IDiagnosticPage Keyword(string keyword) => Raw(keyword);

    /// <summary>
    /// Writes a name to the page.
    /// </summary>
    /// <param name="name">The name to write.</param>
    /// <returns>The current page.</returns>
    IDiagnosticPage Name(string name) => Raw(name);

    /// <summary>
    /// Writes a symbol to the page.
    /// </summary>
    /// <param name="symbol">The symbol to write.</param>
    /// <returns>The current page.</returns>
    IDiagnosticPage Symbol(ISymbol symbol) => Raw(symbol.Name);

    /// <summary>
    /// Writes a location to the page.
    /// </summary>
    /// <param name="location">The location to write.</param>
    /// <returns>The current page.</returns>
    IDiagnosticPage Location(Location location) => Raw(location.ToString());

    /// <summary>
    /// Writes many values onto the page.
    /// </summary>
    /// <param name="writeActions">The actions which write onto the page.</param>
    /// <param name="terminator">Specifies how to terminate the list,
    /// i.e. what to write between the second-to-last and last values instead of a comma.</param>
    /// <returns>The current page.</returns>
    IDiagnosticPage Many(IEnumerable<Action<IDiagnosticPage>> writeActions, ManyTerminator terminator)
    {
        var (dualSeparator, endingSeparator) = terminator switch
        {
            ManyTerminator.And => (" and ", ", and "),
            ManyTerminator.Or => (" or ", ", or "),
            ManyTerminator.None => (", ", ", "),
            _ => throw new UnreachableException()
        };
        
        DiagnosticPageUtility.WriteManyJoinedWithSeparators(
            this,
            page => page.Raw(", "),
            page => page.Raw(dualSeparator),
            page => page.Raw(endingSeparator),
            writeActions);

        return this;
    }
}

/// <summary>
/// Specifies how to terminate a list of many values written onto a page,
/// i.e. what to write between the second-to-last and last values instead of a comma.
/// </summary>
public enum ManyTerminator
{
    /// <summary>
    /// Write an 'and'.
    /// </summary>
    And,
    /// <summary>
    /// Write an 'or'.
    /// </summary>
    Or,
    /// <summary>
    /// Write nothing, or usually a ','.
    /// </summary>
    None
}

/// <summary>
/// A <see cref="IDiagnosticPage"/> which can write its contents as a specific type.
/// </summary>
/// <typeparam name="T">The type of the contents written onto the page.</typeparam>
public interface IDiagnosticPage<out T> : IDiagnosticPage
{
    /// <summary>
    /// Writes the contents of the page as the type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>The written contents of the page.</returns>
    T Write();
}

public static class DiagnosticPageUtility
{
    /// <summary>
    /// Acts as a helper for <see cref="IDiagnosticPage.Many"/>.
    /// Writes many values onto a page using an individual separator
    /// between two values, between the two ending values,
    /// and between the single two values of the sequence if it only contains two values.
    /// </summary>
    /// <param name="page">The page to write to.</param>
    /// <param name="writeSeparator">Writes a separator between values before the ending pair.</param>
    /// <param name="writeEndingSeparator">Writes a separator between the ending pair.</param>
    /// <param name="writeDualSeparator">
    /// Writes a separator between the two values in the sequence if the sequence of contains two values.
    /// </param>
    /// <param name="writeActions">The actions to write.</param>
    public static void WriteManyJoinedWithSeparators(
        IDiagnosticPage page,
        Action<IDiagnosticPage> writeSeparator,
        Action<IDiagnosticPage> writeDualSeparator,
        Action<IDiagnosticPage> writeEndingSeparator,
    IEnumerable<Action<IDiagnosticPage>> writeActions)
    {
        using var enumerator = writeActions.GetEnumerator();

        if (!enumerator.MoveNext()) return;
        var first = enumerator.Current;

        first(page);
        if (!enumerator.MoveNext()) return;
        var second = enumerator.Current;
        
        var count = 2;
        var previous = second;
        while (enumerator.MoveNext())
        {
            writeSeparator(page);
            previous(page);
            
            count++;
            previous = enumerator.Current;
        }

        if (count == 2)
        {
            writeDualSeparator(page);
            previous(page);
        }
        else if (count > 2)
        {
            writeEndingSeparator(page);
            previous(page);
        }
    }

    /// <summary>
    /// Turns a sequence of values into a sequence of actions onto a page.
    /// </summary>
    /// <param name="values">The values to turns into actions.</param>
    /// <param name="action">The action to apply to the page for each value.</param>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <returns>A sequence of page actions.</returns>
    public static IEnumerable<Action<IDiagnosticPage>> ToPageActions<T>(
        IEnumerable<T> values,
        Action<T, IDiagnosticPage> action) =>
        values.Select(Action<IDiagnosticPage> (x) => p => action(x, p));
}
