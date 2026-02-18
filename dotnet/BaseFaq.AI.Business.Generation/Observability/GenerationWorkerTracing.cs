using System.Diagnostics;

namespace BaseFaq.AI.Business.Generation.Observability;

public static class GenerationWorkerTracing
{
    public const string SourceName = "BaseFaq.AI.Generation.Worker";
    public static readonly ActivitySource ActivitySource = new(SourceName);
}