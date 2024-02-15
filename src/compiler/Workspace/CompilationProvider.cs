namespace Noa.Compiler.Workspace;

/// <summary>
/// An asynchronous provider for a compilation.
/// </summary>
/// <param name="path"></param>
internal readonly struct CompilationProvider(string path) : IAsyncDisposable
{
    private readonly TaskCompletionSource completion = new();
    private readonly CancellationTokenSource providerCancellationSource = new();
    
    public DateTimeOffset StartTime { get; } = DateTimeOffset.UtcNow;

    public Task GetCompilation() => completion.Task;

    /// <summary>
    /// Creates a new compilation provider and starts a compilation.
    /// </summary>
    /// <param name="path">The path of the file to compile.</param>
    /// <param name="cancellationToken">The cancellation token for the compilation.</param>
    public static CompilationProvider CreateProvider(string path, CancellationToken cancellationToken)
    {
        var provider = new CompilationProvider(path);
        
        _ = Task.Run(() => provider.Compile(cancellationToken), cancellationToken);

        return provider;
    }

    private async Task Compile(CancellationToken cancellationToken)
    {
        try
        {
            // Create a cancellation token source which cancels when either the provider
            // cancellation source or the passed in cancellation token are cancelled.
            using var compilationCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(
                providerCancellationSource.Token,
                cancellationToken);

            // Todo: compilation here
            await Task.Delay(5000, compilationCancellationSource.Token);

            completion.SetResult();
        }
        catch (Exception e)
        {
            completion.SetException(e);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await providerCancellationSource.CancelAsync();

        try
        {
            await completion.Task;
        }
        // ReSharper disable once EmptyGeneralCatchClause
        catch
        {
            // Exceptions are handled by the catch in Compile.
        }
        finally
        {
            providerCancellationSource.Dispose();
        }
    }
}
