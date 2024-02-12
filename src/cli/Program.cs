using Cocona;
using Cocona.Lite;
using Noa.Cli;
using Spectre.Console;

var builder = CoconaLiteApp.CreateBuilder();

var console = AnsiConsole.Create(new AnsiConsoleSettings());
builder.Services.AddSingleton(console);

var app = builder.Build();

app.AddCommands<Compile>();

app.Run();
