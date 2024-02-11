using Noa.Compiler;
using Cocona;
using Cocona.Lite;
using Noa.Compiler.Diagnostics;
using Noa.Compiler.FlowAnalysis;
using Spectre.Console;
using Noa.Compiler.Symbols;

var builder = CoconaLiteApp.CreateBuilder();

var console = AnsiConsole.Create(new AnsiConsoleSettings());
builder.Services.AddSingleton(console);

var app = builder.Build();

app.AddCommand((
    IAnsiConsole console,
    [Option("input-file", ['i'], Description = "The file to run")] string inputFile) =>
{
    if (!File.Exists(inputFile))
    {
        console.WriteLine($"File '{inputFile}' does not exist.");
        return 1;
    }

    var text = File.ReadAllText(inputFile);
    var file = new FileInfo(inputFile);
    var name = file.Name;
    var source = new Source(text, name);

    var ast = Ast.Create(source);
    var diagnostics = ast.Diagnostics;

    if (diagnostics.Count > 0)
    {
        foreach (var diagnostic in diagnostics)
        {
            var color = diagnostic.Severity switch
            {
                Severity.Warning => Color.Yellow,
                Severity.Error => Color.Red,
                _ => Color.White
            };
            console.Write(new Text($"{diagnostic.Id}: {diagnostic.Message} ({diagnostic.Location})\n", color));
        }
    }
    else
    {
        console.MarkupLine("[green]Success[/]");
    }
    
    return 0;
}).WithDescription("Compiles a file");

app.Run();
