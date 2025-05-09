using System.CommandLine;
using System.CommandLine.Help;
using System.Text;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Noa.Cli;

internal sealed class Help
{
    private readonly IAnsiConsole console;

    private Help(IAnsiConsole console) =>
        this.console = console;

    public static HelpBuilder CreateBuilder(IAnsiConsole console)
    {
        var builder = new HelpBuilder();

        builder.CustomizeLayout(ctx => GetLayout(ctx, console));

        return builder;
    }

    private static IEnumerable<Func<HelpContext, bool>> GetLayout(HelpContext ctx, IAnsiConsole console)
    {
        var help = new Help(console);

        yield return Section(help.Description);
        yield return Section(help.Outline);
        yield return Section(help.Arguments);
        yield return Section(help.Options);
        yield return Section(help.SubcommandsOutline);
        yield return Section(help.Subcommands);
    }

    private static Func<HelpContext, bool> Section(Func<Command, bool> section) =>
        ctx => section(ctx.Command);

    private static IRenderable ShowDualList<TLeft, TRight>(IEnumerable<(TLeft, TRight)> xs)
        where TLeft : IRenderable
        where TRight : IRenderable
    {
        var leftColumn = new GridColumn().Padding(0, 0);
        var rightColumn = new GridColumn().Padding(3, 0, 0, 0);
        
        var grid = new Grid()
            .AddColumns(leftColumn, rightColumn)
            .Collapse();

        foreach (var (left, right) in xs)
        {
            grid.AddRow(left, right);
        }

        return grid;
    }

    private static IRenderable ShowFullCommandName(Command command)
    {
        var names = command.Parents.Reverse().Append(command).Select(x => x.Name);
        return new Text(string.Join(' ', names), Color.White);
    }

    private static IRenderable ShowArgument(Argument arg) => new Markup(
        arg.Arity.MaximumNumberOfValues > 1
            ? $"[[<[purple]{arg.Name}[/]>...]]"
            : $"<[purple]{arg.Name}[/]>");
    
    private static string ShowOptionValues(Option option)
    {
        if (option is not IExtraHelpOption { HelpValue: {} helpValue }) return "";

        var values = helpValue.Split('|', StringSplitOptions.TrimEntries);
        var formatted = values.Select(x => $"[b]{x}[/]");
        return $" <{string.Join("|", formatted)}>";
    }

    private bool Description(Command command)
    {
        if (command.Description is null) return false;

        console.MarkupLine(command.Description);

        return true;
    }

    private bool Outline(Command command)
    {
        console.Write(ShowFullCommandName(command));

        foreach (var arg in command.Arguments)
        {
            console.Write(" ");
            console.Write(ShowArgument(arg));
        }

        console.Markup(" [[[yellow]options[/]]]");

        console.WriteLine();

        return true;
    }

    private bool SubcommandsOutline(Command command)
    {
        if (command.Subcommands.Count == 0 || command.Arguments.Count == 0) return false;

        console.Write(ShowFullCommandName(command));

        console.Markup($" <[green]command[/]> [[[purple]command arguments[/]]] [[[yellow]command options[/]]]");

        console.WriteLine();

        return true;
    }

    private bool Options(Command command)
    {
        if (command.Options.Count == 0) return false;

        var list = ShowDualList(
            command.Options.Select(option =>
            {
                var ns = option.Aliases.Prepend(option.Name)
                    .Select(s => $"[yellow]{s}[/]");
                
                var values = ShowOptionValues(option);
                
                return (
                    new Markup(string.Join("|", ns) + values),
                    new Markup(option.Description ?? " "));
            }));
        
        console.MarkupLine("[white]options[/]:");
        console.Write(new Padder(list).Padding(2, 0, 0, 0));
            
        return true;
    }

    private bool Arguments(Command command)
    {
        if (command.Arguments.Count == 0) return false;

        var list = ShowDualList(
            command.Arguments.Select(arg => (
                ShowArgument(arg),
                new Markup(arg.Description ?? " "))));
        
        console.MarkupLine("[white]arguments[/]:");
        console.Write(new Padder(list).Padding(2, 0, 0, 0));

        return true;
    }

    private bool Subcommands(Command command)
    {
        if (command.Subcommands.Count == 0) return false;

        var list = ShowDualList(
            command.Subcommands.Select(command => (
                new Text(command.Name, Color.Green),
                new Markup(command.Description ?? " "))));
        
        console.MarkupLine("[white]commands[/]:");
        console.Write(new Padder(list).Padding(2, 0, 0, 0));

        return true;
    }
}
