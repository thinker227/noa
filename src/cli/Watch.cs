using Cocona;
using Spectre.Console;

namespace Noa.Cli;

public sealed class Watch(IAnsiConsole console, CancellationToken ct) : CommandBase(console)
{
    [Command("watch", Description = "Watches a file for changes and re-compiles the file upon a change")]
    public async Task<int> Execute(
        [Argument("input-file", Description = "The file to watch for changes")]
        string inputFile)
    {
        await RunWatch(inputFile, ct);
        
        return 0;
    }
    
    private async Task RunWatch(string inputFile, CancellationToken ct)
    {
        var file = new FileInfo(inputFile);
        
        using var watcher = new FileSystemWatcher();
        watcher.Path = file.Directory!.FullName;
        watcher.Filter = file.Name;
        watcher.EnableRaisingEvents = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;

        var taskCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var taskCt = taskCts.Token;

        var displayPath = GetDisplayPath(file);
        console.Write(DisplayStartupInfo(displayPath));
        console.WriteLine();
    
        // Todo: this is a very very bad approach, please find a better solution.
        
        // Certain editors trigger multiple changes to a file when saved.
        // To avoid compiling the same code multiple times, we use this awkward task approach.
        
        var compileTask = null as Task;
    
        watcher.Changed += (_, _) =>
        {
            if (!compileTask?.IsCompleted ?? false) return;
    
            compileTask = Task.Run(RunCompile, ct);
        };

        watcher.Deleted += (_, _) =>
        {
            console.MarkupLine($"{Emoji.Known.Wastebasket} [aqua]{displayPath}[/] was deleted.");
            
            taskCts.Cancel();
        };
    
        try
        {
            await Task.Delay(-1, taskCt);
        }
        catch (TaskCanceledException) {}
    
        console.MarkupLine($"{Emoji.Known.StopSign} Stopped.");
    
        return;
    
        void RunCompile()
        {
            console.MarkupLine($"{Emoji.Known.Wrench} Building... ");
    
            var (ast, time) = CoreCompile(file, taskCt);

            console.Write(DisplayBuildDuration(time));
            console.WriteLine();
            PrintStatus(ast.Source, ast.Diagnostics);
        }
    }

    private static Rows DisplayStartupInfo(string displayPath)
    {
        var header = new Text($"{Emoji.Known.Eyes} Watch started!");

        var info = new Padder(
            new Rows(
                new Markup(
                    $"Watching for changes in [aqua]{displayPath}[/]. " +
                    "Update the file to trigger a recompile.", Color.Grey),
                new Markup("Press [aqua]ctrl+c[/] to cancel.", Color.Grey)))
            .Padding(3, 0, 0, 0);

        return new Rows(header, info);
    }
}
