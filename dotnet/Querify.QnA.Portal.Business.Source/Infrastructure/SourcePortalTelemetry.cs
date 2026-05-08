using System.Diagnostics;

namespace Querify.QnA.Portal.Business.Source.Infrastructure;

public static class SourcePortalTelemetry
{
    public const string ActivitySourceName = "Querify.QnA.Portal.Source";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
