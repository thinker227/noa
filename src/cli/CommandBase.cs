using System.Diagnostics;
using Noa.Compiler;
using Noa.Compiler.Diagnostics;
using Spectre.Console;

namespace Noa.Cli;

public class CommandBase
{
    protected static string GetDisplayPath(FileInfo file) =>
        Path.GetRelativePath(Environment.CurrentDirectory, file.FullName);
    
    protected static (Ast ast, TimeSpan time) CoreCompile(FileInfo file)
    {
        var text = File.ReadAllText(file.FullName);
        var name = GetDisplayPath(file);
        var source = new Source(text, name);

        var timer = new Stopwatch();
        timer.Start();
        
        var ast = Ast.Create(source);

        timer.Stop();
        var time = timer.Elapsed;

        return (ast, time);
    }

    protected static void PrintStatus(IAnsiConsole console, IReadOnlyCollection<IDiagnostic> diagnosticsResult)
    {
        var collectedDiagnostics = diagnosticsResult
            .GroupBy(x => x.Severity)
            .ToDictionary(x => x.Key, x => x.ToArray());
        
        var diagnostics = collectedDiagnostics
            .GetValueOrDefault(Severity.Error, [])
            .Concat(collectedDiagnostics.GetValueOrDefault(Severity.Warning, []))
            .OrderBy(x => x.Location.SourceName)
            .ThenBy(x => x.Location.Start)
            .ThenBy(x => x.Location.Length);

        var statusText = DisplayBuildStatusText(collectedDiagnostics);
        var diagnosticsGrid = DisplayDiagnosticsGrid(diagnostics);
        var diagnosticsDisplay = new Padder(diagnosticsGrid).Padding(6, 1, 0, 1);
        
        console.Write(statusText);
        console.WriteLine();
        if (diagnosticsResult.Count > 0) console.Write(diagnosticsDisplay);
    }

    private static Markup DisplayBuildStatusText(IReadOnlyDictionary<Severity, IDiagnostic[]> diagnostics)
    {
        var warnings = diagnostics.GetValueOrDefault(Severity.Warning);
        var errors = diagnostics.GetValueOrDefault(Severity.Error);

        var text = (warnings, errors) switch
        {
            (null, null) =>
                "\u2705 [green]Build succeeded![/]",
            
            (not null, null) =>
                "\u2705 [green]Build succeeded[/] " +
                $"with [yellow]{warnings.Length} warning{Plural(warnings)}[/]",
            
            (null, not null) =>
                "\u274c [red]Build failed[/] " +
                $"with [red]{errors.Length} error{Plural(errors)}[/]",
            
            (not null, not null) =>
                "\u274c [red]Build failed[/] " +
                $"with [red]{errors.Length} error{Plural(errors)}[/] " +
                $"and [yellow]{warnings.Length} warning{Plural(warnings)}[/]"
        };

        return new Markup(text);
        
        static string Plural<T>(IReadOnlyCollection<T> xs) =>
            xs.Count > 1
                ? "s"
                : "";
    }

    private static Grid DisplayDiagnosticsGrid(IEnumerable<IDiagnostic> diagnostics)
    {
        var displays = diagnostics.Select(DisplayDiagnostic);

        var grid = new Grid()
            .AddColumn(new GridColumn()
                .Padding(0, 0, 1, 0))
            .AddColumn(new GridColumn()
                .Padding(0, 0, 0, 0));

        var dash = new Text("-");
        foreach (var display in displays)
        {
            grid.AddRow(dash, display);
        }

        return grid;
    }

    private static Markup DisplayDiagnostic(IDiagnostic diagnostic)
    {
        var location = diagnostic.Location;
        
        var color = diagnostic.Severity switch
        {
            Severity.Warning => Color.Yellow,
            Severity.Error => Color.Red,
            _ => Color.White
        };

        var text = $"[white]{diagnostic.Id}[/] at " +
                   $"[aqua]{location.Start}[/] to [aqua]{location.End}[/] in [aqua]{location.SourceName}[/]\n" +
                   $"[{color.ToMarkup()}]{diagnostic.Message}[/]";
            
        return new Markup(text, Color.Grey);
    }

    protected static Markup DisplayBuildDuration(TimeSpan time)
    {
        var (duration, unit) = time.TotalMilliseconds >= 5
            ? (time.TotalMilliseconds, "ms")
            : (time.TotalMicroseconds, "μs");
        
        return new($"\ud83d\udd52 Build took [aqua]{duration:F0}{unit}[/]");
    }
}
