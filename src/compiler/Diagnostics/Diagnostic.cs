namespace Noa.Compiler.Diagnostics;

/// <summary>
/// Static helpers for creating diagnostics. 
/// </summary>
public static class Diagnostic
{
    /// <summary>
    /// Creates a simple diagnostic from a diagnostic template and a location.
    /// </summary>
    /// <param name="template">The template to create the diagnostic from.</param>
    /// <param name="location">The location of the diagnostic.</param>
    public static IDiagnostic Create(DiagnosticTemplate template, Location location) =>
        new SimpleDiagnostic(template, location);

    /// <summary>
    /// Creates a diagnostic with from a template with an argument and a location.
    /// </summary>
    /// <param name="template">The template to create the diagnostic from.</param>
    /// <param name="argument">The argument to supply to the template.</param>
    /// <param name="location">The location of the diagnostic.</param>
    /// <typeparam name="TArg">The type of the argument to the template.</typeparam>
    public static IDiagnostic Create<TArg>(DiagnosticTemplate<TArg> template, TArg argument, Location location) =>
        new ArgumentDiagnostic<TArg>(template, argument, location);

    /// <summary>
    /// Writes the message of a diagnostic using a specified <see cref="IDiagnosticWriter"/>.
    /// </summary>
    /// <param name="diagnostic">The diagnostic to write the message of.</param>
    /// <param name="writer">The <see cref="IDiagnosticWriter"/> to use to write the message.</param>
    /// <param name="ast">The AST the diagnostic belongs to.</param>
    /// <typeparam name="T">The type of the written message.</typeparam>
    /// <returns>The written diagnostic message.</returns>
    public static T WriteMessage<T>(this IDiagnostic diagnostic, IDiagnosticWriter<T> writer)
    {
        var page = writer.CreatePage(diagnostic);
        diagnostic.WriteToPage(page);
        return page.Write();
    }

    private static string DisplayDiagnostic(DiagnosticId id, string message, Severity severity, Location location)
    {
         var severityString = severity switch
         {
             Severity.Warning => "warning",
             Severity.Error => "error",
             _ => throw new UnreachableException()
         };

         return $"{id}: \"{message}\" ({severityString}) at {location}";
    }
    
    private sealed class SimpleDiagnostic(DiagnosticTemplate template, Location location) : IDiagnostic
    {
        public DiagnosticId Id { get; } = template.Id;
    
        public Severity Severity { get; } = template.Severity;
    
        public Location Location { get; } = location;

        public void WriteToPage(IDiagnosticPage page) => page.Raw(template.Message);

        public override string ToString()
        {
            var message = WriteMessage(this, StringDiagnosticWriter.Writer);
            return DisplayDiagnostic(Id, message, Severity, Location);
        }
    }
    
    private sealed class ArgumentDiagnostic<TArg>(
        DiagnosticTemplate<TArg> template,
        TArg argument,
        Location location) : IDiagnostic
    {
        public DiagnosticId Id { get; } = template.Id;

        public Severity Severity { get; } = template.Severity;

        public Location Location { get; } = location;

        public void WriteToPage(IDiagnosticPage page) => template.WriteMessage(argument, page);

        public override string ToString()
        {
            var message = WriteMessage(this, StringDiagnosticWriter.Writer);
            return DisplayDiagnostic(Id, message, Severity, Location);
        }
    }
}
