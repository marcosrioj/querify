using BaseFaq.AI.Business.Generation.Dtos;
using BaseFaq.AI.Business.Generation.Service;
using BaseFaq.Models.Ai.Contracts.Generation;
using BaseFaq.Models.Faq.Enums;
using Xunit;

namespace BaseFaq.AI.Test.IntegrationTest.Tests.Infrastructure;

public sealed class SimpleFaqGenerationEngineTests
{
    [Fact]
    public void Generate_ComputesConfidenceFromProcessedRatio()
    {
        var engine = new SimpleFaqGenerationEngine();
        var request = new FaqGenerationRequestedV1
        {
            CorrelationId = Guid.NewGuid(),
            FaqId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            RequestedByUserId = Guid.NewGuid(),
            Language = "en",
            IdempotencyKey = "key",
            RequestedUtc = DateTime.UtcNow
        };
        var studiedRefs = new ContentRefStudyResult(
            TotalCount: 4,
            ProcessedCount: 3,
            SkippedCount: 1,
            StudiedRefs:
            [
                new StudiedContentRef(ContentRefKind.Web, "https://a", "a"),
                new StudiedContentRef(ContentRefKind.Video, "https://b", "b"),
                new StudiedContentRef(ContentRefKind.Pdf, "https://c", "c")
            ]);

        var result = engine.Generate(request, studiedRefs);

        Assert.Equal(75, result.Confidence);
    }
}