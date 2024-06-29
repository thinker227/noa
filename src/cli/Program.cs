using Cocona;
using Cocona.Lite;
using Noa.Cli;
using Spectre.Console;

var builder = CoconaLiteApp.CreateBuilder(args, options =>
{
    options.EnableShellCompletionSupport = true;
    options.EnableConvertOptionNameToLowerCase = true;
    options.EnableConvertArgumentNameToLowerCase = true;
});

var cts = new CancellationTokenSource();
builder.Services.AddSingleton(cts.Token);
Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    cts.Cancel();
};

var console = AnsiConsole.Create(new AnsiConsoleSettings());
builder.Services.AddSingleton(console);

var app = builder.Build();

app.AddCommands<Compile>();
app.AddCommands<Watch>();
app.AddCommands<LangServer>();

await app.RunAsync(cts.Token);
