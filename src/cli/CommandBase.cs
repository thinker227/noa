using System.Diagnostics;
using Noa.Compiler;
using Noa.Compiler.Diagnostics;
using Spectre.Console;

namespace Noa.Cli;

public abstract class CommandBase(IAnsiConsole console)
{
    protected readonly IAnsiConsole console = console;
    
    protected static string GetDisplayPath(FileInfo file) =>
        Path.GetRelativePath(Environment.CurrentDirectory, file.FullName);
    
    protected static (Ast ast, TimeSpan time) CoreCompile(FileInfo file, CancellationToken ct)
    {
        var text = File.ReadAllText(file.FullName);
        var name = GetDisplayPath(file);
        var source = new Source(text, name);

        var timer = new Stopwatch();
        timer.Start();
        
        var ast = Ast.Create(source, ct);

        timer.Stop();
        var time = timer.Elapsed;

        return (ast, time);
    }

    protected void PrintStatus(Source source, IReadOnlyCollection<IDiagnostic> diagnosticsResult)
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
        var diagnosticsGrid = DisplayDiagnosticsGrid(source, diagnostics);
        var diagnosticsDisplay = new Padder(diagnosticsGrid).Padding(6, 1, 0, 1);
        
        console.Write(statusText);
        console.WriteLine();
        if (diagnosticsResult.Count > 0)
            console.Write(diagnosticsDisplay);
    }

    private static Markup DisplayBuildStatusText(IReadOnlyDictionary<Severity, IDiagnostic[]> diagnostics)
    {
        var warnings = diagnostics.GetValueOrDefault(Severity.Warning);
        var errors = diagnostics.GetValueOrDefault(Severity.Error);

        var text = (warnings, errors) switch
        {
            (null, null) =>
                $"{Emoji.Known.CheckMarkButton} [green]Build succeeded![/]",
            
            (not null, null) =>
                $"{Emoji.Known.CheckMarkButton} [green]Build succeeded[/] " +
                $"with [yellow]{warnings.Length} warning{Plural(warnings)}[/]",
            
            (null, not null) =>
                $"{Emoji.Known.CrossMark} [red]Build failed[/] " +
                $"with [red]{errors.Length} error{Plural(errors)}[/]",
            
            (not null, not null) =>
                $"{Emoji.Known.CrossMark} [red]Build failed[/] " +
                $"with [red]{errors.Length} error{Plural(errors)}[/] " +
                $"and [yellow]{warnings.Length} warning{Plural(warnings)}[/]"
        };

        return new Markup(text);
        
        static string Plural<T>(IReadOnlyCollection<T> xs) =>
            xs.Count > 1
                ? "s"
                : "";
    }

    private static Grid DisplayDiagnosticsGrid(Source source, IEnumerable<IDiagnostic> diagnostics)
    {
        // Todo: this is a massive hack to avoid getting out of range exceptions.
        var lineMap = LineMap.Create(source.Text + " ");

        var grid = new Grid()
            .AddColumn(new GridColumn()
                .Padding(0, 0, 1, 0))
            .AddColumn(new GridColumn()
                .Padding(0, 0, 0, 0));

        var dash = new Text("-");
        foreach (var diagnostic in diagnostics)
        {
            var display = DisplayDiagnostic(lineMap, diagnostic);
            grid.AddRow(dash, display);
        }

        return grid;
    }

    private static Markup DisplayDiagnostic(LineMap lineMap, IDiagnostic diagnostic)
    {
        var location = diagnostic.Location;
        var start = lineMap.GetCharacterPosition(location.Start);
        var end = lineMap.GetCharacterPosition(location.End);
        
        var color = diagnostic.Severity switch
        {
            Severity.Warning => Color.Yellow,
            Severity.Error => Color.Red,
            _ => Color.White
        };

        var message = diagnostic.WriteMessage(SpectreDiagnosticWriter.Writer);
        var text = $"[white]{diagnostic.Id}[/] at " +
                   $"[aqua]{start.Line.LineNumber}:{start.Offset + 1}[/] to " +
                   $"[aqua]{end.Line.LineNumber}:{end.Offset + 1}[/] " +
                   $"in [aqua]{location.SourceName}[/]\n" +
                   $"[{color.ToMarkup()}]{message}[/]";
            
        return new Markup(text, Color.Grey);
    }

    protected static Markup DisplayBuildDuration(TimeSpan time)
    {
        var duration = DisplayDuration(time);
        
        return new($"{Emoji.Known.Stopwatch}  Build took [aqua]{duration}[/]");
    }

    protected static string DisplayDuration(TimeSpan time) =>
        (time.TotalMilliseconds, time.TotalSeconds, time.TotalMinutes, time.TotalHours) switch
        {
            (< 5, _, _, _) => $"{time.TotalMicroseconds:F0}μs",
            (_, < 1, _, _) => $"{time.TotalMilliseconds:F0}ms",
            (_, < 10, _, _) => $"{time.Seconds}s{time.Milliseconds}ms",
            (_, _, < 1, _) => $"{time.Seconds}s",
            (_, _, >= 1, < 1) => $"{time.Minutes}m{time.Seconds}s",
            _ => $"{time.Hours}h{time.Minutes}m{time.Seconds}s"
        };
}
