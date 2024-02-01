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

    return 0;
});

app.Run();
