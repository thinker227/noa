using System.CommandLine;

namespace Noa.Cli;

public interface IExtraHelpOption
{
    string? HelpValue { get; }
}

public sealed class ExtraHelpOption<T>(
    string name,
    params string[] aliases)
    : Option<T>(name, aliases), IExtraHelpOption
{
    public string? HelpValue { get; set; } = null;
}
