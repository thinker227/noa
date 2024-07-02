using Draco.Lsp.Model;
using Draco.Lsp.Server;
using Noa.LangServer.Logging;
using Serilog;
using Serilog.Core;

namespace Noa.LangServer;

public sealed partial class NoaLanguageServer
{
    /// <summary>
    /// Creates and runs a new instance of the language server.
    /// </summary>
    /// <param name="logFilePath">
    /// The path to the file to output log messages to.
    /// Will not output log messages to a file if not specified.
    /// </param>
    public static async Task RunAsync(string? logFilePath)
    {
        var stream = new StdioDuplexPipe();
        var client = LanguageServer.Connect(stream);
        
        var logger = CreateLogger(logFilePath, client);
        
        var server = new NoaLanguageServer(client, logger);
        await client.RunAsync(server);
    }

    private static Logger CreateLogger(string? logFilePath, ILanguageClient client)
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
