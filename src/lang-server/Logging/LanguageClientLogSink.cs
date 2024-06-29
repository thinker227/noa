using Draco.Lsp.Model;
using Draco.Lsp.Server;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Noa.LangServer.Logging;

internal sealed class LanguageClientLogSink(ILanguageClient client) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();

        var type = logEvent.Level switch
        {
            LogEventLevel.Verbose => MessageType.Info,
            LogEventLevel.Debug => MessageType.Info,
            LogEventLevel.Information => MessageType.Info,
            LogEventLevel.Warning => MessageType.Warning,
            LogEventLevel.Error => MessageType.Error,
            LogEventLevel.Fatal => MessageType.Error,
            _ => MessageType.Info
        };
        
        client.LogMessageAsync(new LogMessageParams()
        {
            Message = message,
            Type = type
        });
    }
}

internal static class LoggerSinkConfigurationExtensions
{
    public static LoggerConfiguration LanguageClient(this LoggerSinkConfiguration config, ILanguageClient client) =>
        config.Sink(new LanguageClientLogSink(client));
}
