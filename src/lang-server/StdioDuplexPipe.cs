using System.IO.Pipelines;

namespace Noa.LangServer;

internal sealed class StdioDuplexPipe : IDuplexPipe
{
    public PipeReader Input { get; } = PipeReader.Create(Console.OpenStandardInput());

    public PipeWriter Output { get; } = PipeWriter.Create(Console.OpenStandardOutput());
}
