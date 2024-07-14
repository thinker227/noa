using System.Diagnostics;
using Draco.Lsp.Model;
using Draco.Lsp.Server;
using Noa.LangServer.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

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
    /// <param name="logLevel">The level of the messages to log from the server.</param>
    public static async Task RunAsync(string? logFilePath, LogLevel logLevel)
    {
        var stream = new StdioDuplexPipe();
        var client = LanguageServer.Connect(stream);
        
        var logger = CreateLogger(logFilePath, logLevel, client);
        
        var server = new NoaLanguageServer(client, logger);
        await client.RunAsync(server);
    }

    private static Logger CreateLogger(string? logFilePath, LogLevel logLevel, ILanguageClient client)
    {
        var config = new LoggerConfiguration();
        
        config.Enrich.FromLogContext();

        config.WriteTo.LanguageClient(client);
        if (logFilePath is not null) config.WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day);

        var loggingLevelSwitch = new LoggingLevelSwitch(logLevel switch
        {
            LogLevel.Info => LogEventLevel.Information,
            LogLevel.Debug => LogEventLevel.Verbose,
            _ => throw new UnreachableException()
        });
        config.MinimumLevel.ControlledBy(loggingLevelSwitch);

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
