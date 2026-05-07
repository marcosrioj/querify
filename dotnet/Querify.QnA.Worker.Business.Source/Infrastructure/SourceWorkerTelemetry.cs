using System.Diagnostics;

namespace Querify.QnA.Worker.Business.Source.Infrastructure;

public static class SourceWorkerTelemetry
{
    public const string ActivitySourceName = "Querify.QnA.Worker.Source";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
