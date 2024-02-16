// ReSharper disable MethodSupportsCancellation
#pragma warning disable CA2016

namespace Noa.Compiler.Workspace;

/// <summary>
/// Manages compilations with thread safety.
/// </summary>
/// <param name="debounceThreshold">
/// The threshold within which to reject new compilation of the same file.
/// The default is 50ms.
/// </param>
public sealed class CompilationManager(TimeSpan? debounceThreshold = null)
{
    private readonly TimeSpan debounceThreshold = debounceThreshold ?? TimeSpan.FromMilliseconds(50);
    private readonly SemaphoreSlim @lock = new(1, 1);
    private readonly Dictionary<ISourceProvider, CompilationProvider> pendingCompilations = new();

    /// <summary>
    /// Compiles a source from a source provider.
    /// </summary>
    /// <param name="sourceProvider">The source provider which provides the source to compile.</param>
    /// <param name="cancellationToken">The cancellation token for the compilation.</param>
    public async Task<Ast> Compile(ISourceProvider sourceProvider, CancellationToken cancellationToken = default)
    {
        CompilationProvider provider;

        // The entire following block should only be executed once simultaneously.
        await @lock.WaitAsync();
        try
        {
            if (pendingCompilations.TryGetValue(sourceProvider, out provider))
            {
                if (DateTimeOffset.UtcNow - provider.StartTime > debounceThreshold)
                {
                    // Cancel the currently running compilation and start a new one.
                    await provider.DisposeAsync();

                    var source = sourceProvider.GetSource();
                    provider = CompilationProvider.CreateProvider(source, cancellationToken);
                    pendingCompilations[sourceProvider] = provider;
                }
            }
            else
            {
                // There is no currently running compilation, start a new one.
                var source = sourceProvider.GetSource();
                provider = CompilationProvider.CreateProvider(source, cancellationToken);
                pendingCompilations[sourceProvider] = provider;
            }
        }
        finally
        {
            @lock.Release();
        }

        try
        {
            return await provider.GetCompilation();
        }
        finally
        {
            await @lock.WaitAsync();
            try
            {
                pendingCompilations.Remove(sourceProvider);
            }
            finally
            {
                @lock.Release();
            }
        }
    }
}
