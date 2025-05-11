using Noa.Cli;
using Noa.Cli.Commands;
using Spectre.Console;

var console = AnsiConsole.Create(new AnsiConsoleSettings());

var help = Help.CreateBuilder(console);
var root = Root.CreateCommand(help, console);

return root.Parse(args).Invoke();
