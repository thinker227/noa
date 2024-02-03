using Noa.Compiler;
using Cocona;
using Cocona.Lite;
using Spectre.Console;

var builder = CoconaLiteApp.CreateBuilder();

var console = AnsiConsole.Create(new AnsiConsoleSettings());
builder.Services.AddSingleton(console);

var app = builder.Build();

app.AddCommand((
    IAnsiConsole console,
    [Argument(Description = "The file to run")] string inputFile) =>
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

    foreach (var diagnostic in ast.Diagnostics)
    {
        var color = diagnostic.Severity switch
        {
            Severity.Warning => Color.Yellow,
            Severity.Error => Color.Red,
            _ => Color.White
        };
        console.Write(new Text($"{diagnostic.Message} ({diagnostic.Location})\n", color));
    }
    
    return 0;
});

app.Run();
