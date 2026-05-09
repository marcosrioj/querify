using Querify.Common.EntityFramework.Core.AutoHistory;
using Querify.Models.QnA.Dtos.Source;
using Querify.QnA.Portal.Business.Source.Commands.CreateSource;
using Querify.QnA.Portal.Business.Source.Commands.UpdateSource;
using Querify.QnA.Portal.Business.Source.Queries.GetSource;
using Querify.QnA.Portal.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Querify.QnA.Portal.Test.IntegrationTests.Tests.Source;

public class SourceCommandQueryTests
{
    [Fact]
    public async Task CreateSource_PersistsEntityAndReturnsDto()
    {
        using var context = TestContext.Create();

        var createHandler =
            new SourcesCreateSourceCommandHandler(context.DbContext, context.SessionService);
        var id = await createHandler.Handle(new SourcesCreateSourceCommand
        {
            Request = new SourceCreateRequestDto
            {
                Locator = "https://docs.example.test/qna/reset-password",
                Label = "Reset password guide",
                ContextNote = "Portal",
                ExternalId = "DOC-RESET-1",
                Language = "en-US",
                MediaType = "text/html",
                MetadataJson = "{\"category\":\"support\"}"
            }
        }, CancellationToken.None);

        var getHandler = new SourcesGetSourceQueryHandler(context.DbContext, context.SessionService);
        var result = await getHandler.Handle(new SourcesGetSourceQuery { Id = id },
            CancellationToken.None);

        Assert.Equal("https://docs.example.test/qna/reset-password", result.Locator);
        Assert.Equal("Reset password guide", result.Label);
        Assert.Equal("DOC-RESET-1", result.ExternalId);
        Assert.StartsWith("sha256:", result.Checksum);
        Assert.Equal(71, result.Checksum.Length);

        var history = await context.DbContext.Set<AutoHistory>().SingleAsync();
        Assert.Equal(id.ToString(), history.KeyId);
        Assert.Equal("Sources", history.TableName);
        Assert.Equal(EntityState.Added, history.Kind);
        Assert.Null(history.ChangedFrom);
        Assert.Contains("https://docs.example.test/qna/reset-password", history.ChangedTo);
    }

    [Fact]
    public async Task UpdateSource_DoesNotChangeLocatorOrChecksum()
    {
        using var context = TestContext.Create();
        var source = await TestDataFactory.SeedSourceAsync(context.DbContext, context.SessionService.TenantId);
        var originalLocator = source.Locator;
        var originalChecksum = source.Checksum;

        var updateHandler =
            new SourcesUpdateSourceCommandHandler(context.DbContext, context.SessionService);
        await updateHandler.Handle(new SourcesUpdateSourceCommand
        {
            Id = source.Id,
            Request = new SourceUpdateRequestDto
            {
                Label = source.Label,
                ContextNote = source.ContextNote,
                ExternalId = "DOC-2",
                Language = source.Language,
                MediaType = source.MediaType,
                MetadataJson = source.MetadataJson
            }
        }, CancellationToken.None);

        Assert.Equal(originalLocator, source.Locator);
        Assert.Equal("DOC-2", source.ExternalId);
        Assert.Equal(originalChecksum, source.Checksum);
    }

}
