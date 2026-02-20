using BaseFaq.AI.Business.Generation.Service;
using BaseFaq.Models.Faq.Enums;
using Xunit;

namespace BaseFaq.AI.Test.IntegrationTest.Tests.Infrastructure;

public class ContentRefStudyServiceTests
{
    [Fact]
    public void Study_Processes_Only_Web_Pdf_Document_And_Video()
    {
        var refs = new (ContentRefKind Kind, string Locator)[]
        {
            (ContentRefKind.Manual, "manual://skip"),
            (ContentRefKind.Web, "https://www.example.com/web"),
            (ContentRefKind.Pdf, "https://www.example.com/file.pdf"),
            (ContentRefKind.Document, "doc://123"),
            (ContentRefKind.Video, "https://www.example.com/video"),
            (ContentRefKind.Faq, "faq://skip"),
            (ContentRefKind.FaqItem, "faq-item://skip"),
            (ContentRefKind.Other, "other://skip")
        };

        var service = new ContentRefStudyService();
        var result = service.Study(refs);

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
        var refs = new (ContentRefKind Kind, string Locator)[]
        {
            (ContentRefKind.Web, " https://www.example.com/guide ")
        };

        var service = new ContentRefStudyService();
        var result = service.Study(refs);
        var studied = Assert.Single(result.StudiedRefs);

        Assert.Equal("https://www.example.com/guide", studied.Locator);
        Assert.Contains("https://www.example.com/guide", studied.MainSubject);
    }
}
