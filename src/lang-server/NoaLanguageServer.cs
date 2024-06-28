// ReSharper disable VariableHidesOuterVariable
// ReSharper disable UnusedParameter.Local

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;
using Serilog.Core;

namespace Noa.LangServer;

public sealed class NoaLanguageServer
{
    public static async Task RunAsync(string logFilePath, CancellationToken ct)
    {
        var server = await LanguageServer.From(
            options => ConfigureServerOptions(options, logFilePath),
            ct).ConfigureAwait(false);

        await server.WaitForExit.ConfigureAwait(false);
    }

    private static void ConfigureServerOptions(LanguageServerOptions options, string logFilePath)
    {
        var logger = CreateLogger(logFilePath);

        logger.Information("Configuring server");

        options.ConfigureLogging(x => x
            .AddSerilog(logger)
            .AddLanguageProtocolLogging()
            .SetMinimumLevel(LogLevel.Debug));

        options.WithInput(Console.OpenStandardInput());
        options.WithOutput(Console.OpenStandardOutput());

        options.OnInitialize(async (server, request, ct) =>
        {
            var logger = server.Services.GetRequiredService<ILogger<NoaLanguageServer>>();
            logger.LogInformation("Initializing");
        });

        options.OnInitialized(async (server, request, response, ct) =>
        {
            var logger = server.Services.GetRequiredService<ILogger<NoaLanguageServer>>();
            logger.LogInformation("Initialized");
        });
    }

    private static Logger CreateLogger(string logFilePath) =>
        new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(logFilePath)
            .MinimumLevel.Verbose()
            .CreateLogger();
}
