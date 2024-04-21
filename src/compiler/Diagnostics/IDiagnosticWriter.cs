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
    /// Writes a single character to the page.
    /// </summary>
    /// <param name="character">The character to write.</param>
    /// <returns>The current page.</returns>
    IDiagnosticPage Character(char character) => Raw(character.ToString());

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

public static class DiagnosticWriterExtensions
{
    /// <summary>
    /// Writes a diagnostic message.
    /// </summary>
    /// <param name="writer">The diagnostic writer.</param>
    /// <param name="diagnostic">The diagnostic to write the message of.</param>
    /// <typeparam name="T">The type of the written message.</typeparam>
    /// <returns>The written diagnostic message.</returns>
    public static T Write<T>(this IDiagnosticWriter<T> writer, IDiagnostic diagnostic) =>
        writer.CreatePage(diagnostic).Write();
}
