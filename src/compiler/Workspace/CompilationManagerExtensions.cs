namespace Noa.Compiler.Workspace;

public static class CompilationManagerExtensions
{
    /// <summary>
    /// Compiles a source from a source provider with a given timeout.
    /// </summary>
    /// <param name="manager">The compilation manager.</param>
    /// <param name="sourceProvider">The source provider which provides the source to compile.</param>
    /// <param name="timeout">The timeout for the compilation.</param>
    public static async Task Compile(
        this CompilationManager manager,
        ISourceProvider sourceProvider,
        TimeSpan timeout)
    {
        var cancellationTokenSource = new CancellationTokenSource(timeout);
        await manager.Compile(sourceProvider, cancellationTokenSource.Token);
    }
}
