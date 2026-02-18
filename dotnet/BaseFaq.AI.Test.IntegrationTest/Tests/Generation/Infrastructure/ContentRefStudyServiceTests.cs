using BaseFaq.AI.Business.Generation.Service;
using BaseFaq.Models.Faq.Enums;
using Xunit;

namespace BaseFaq.AI.Test.IntegrationTest.Tests.Infrastructure;

public class ContentRefStudyServiceTests
{
    [Fact]
    public void Study_Processes_Only_Web_Pdf_Document_And_Video()
    {
        var refs = new[]
        {
            new ContentRefStudyInput(ContentRefKind.Manual, "manual://skip"),
            new ContentRefStudyInput(ContentRefKind.Web, "https://example.test/web"),
            new ContentRefStudyInput(ContentRefKind.Pdf, "https://example.test/file.pdf"),
            new ContentRefStudyInput(ContentRefKind.Document, "doc://123"),
            new ContentRefStudyInput(ContentRefKind.Video, "https://example.test/video"),
            new ContentRefStudyInput(ContentRefKind.Faq, "faq://skip"),
            new ContentRefStudyInput(ContentRefKind.FaqItem, "faq-item://skip"),
            new ContentRefStudyInput(ContentRefKind.Other, "other://skip")
        };

        var result = ContentRefStudyService.Study(refs);

        Assert.Equal(8, result.TotalCount);
        Assert.Equal(4, result.ProcessedCount);
        Assert.Equal(4, result.SkippedCount);
        Assert.Collection(
            result.StudiedRefs,
            item => Assert.Equal(ContentRefKind.Web, item.Kind),
            item => Assert.Equal(ContentRefKind.Pdf, item.Kind),
            item => Assert.Equal(ContentRefKind.Document, item.Kind),
            item => Assert.Equal(ContentRefKind.Video, item.Kind));
    }

    [Fact]
    public void Study_Uses_Locator_As_Source_For_MainSubject()
    {
        var refs = new[]
        {
            new ContentRefStudyInput(ContentRefKind.Web, " https://example.test/guide ")
        };

        var result = ContentRefStudyService.Study(refs);
        var studied = Assert.Single(result.StudiedRefs);

        Assert.Equal("https://example.test/guide", studied.Locator);
        Assert.Contains("https://example.test/guide", studied.MainSubject);
    }
}