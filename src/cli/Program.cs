using Cocona;
using Cocona.Lite;
using Noa.Cli;
using Noa.Compiler;
using Noa.Compiler.Syntax;
using Spectre.Console;

var source = """
mut 1
""";
var ast = Ast.Create(new Source(source, "uwu"));
ast.SyntaxRoot.GetLeftTokenAt(4);

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
app.AddCommands<Run>();
app.AddCommands<Watch>();
app.AddCommands<LangServer>();

await app.RunAsync(cts.Token);
