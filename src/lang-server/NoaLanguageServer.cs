using Draco.Lsp.Model;
using Draco.Lsp.Server;
using Noa.LangServer.Logging;
using Serilog;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer(
    ILanguageClient client,
    ILogger logger,
    CancellationToken cancellationToken)
    : ILanguageServer
{
    public InitializeResult.ServerInfoResult? Info { get; } = new()
    {
        Name = "Noa Language Server",
        Version = "0.1.0"
    };

    public static IList<DocumentFilter> DocumentSelector { get; } =
    [
        new()
        {
            Language = "noa",
            Pattern = "**/*.noa"
        }
    ];

    public static async Task RunAsync(string? logFilePath, CancellationToken cancellationToken)
    {
        var stream = new StdioDuplexPipe();
        var client = LanguageServer.Connect(stream);
        
        var logger = CreateLogger(logFilePath, client);
        
        var server = new NoaLanguageServer(client, logger, cancellationToken);
        await client.RunAsync(server);
    }

    private static ILogger CreateLogger(string? logFilePath, ILanguageClient client)
    {
        var config = new LoggerConfiguration();
        
        config.Enrich.FromLogContext();

        config.WriteTo.LanguageClient(client);
        if (logFilePath is not null) config.WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day);

        config.MinimumLevel.Verbose();

        return config.CreateLogger();
    }

    public Task InitializeAsync(InitializeParams param)
    {
        logger.Information("Initializing server.");
        return Task.CompletedTask;
    }

    public Task InitializedAsync(InitializedParams param)
    {
        logger.Information("Server initialized.");
        return Task.CompletedTask;
    }

    public Task ShutdownAsync()
    {
        logger.Information("Shutting down server.");
        return Task.CompletedTask;
    }
    
    public void Dispose() {}
}
