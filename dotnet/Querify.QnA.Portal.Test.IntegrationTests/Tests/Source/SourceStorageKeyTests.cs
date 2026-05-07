using Querify.QnA.Common.Domain.BusinessRules.Sources;
using Xunit;

namespace Querify.QnA.Portal.Test.IntegrationTests.Tests.Source;

public class SourceStorageKeyTests
{
    [Fact]
    public void BuildStagingKey_StripsPathAndSanitizesFileName()
    {
        var tenantId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();

        var key = SourceStorageKey.BuildStagingKey(tenantId, sourceId, @"C:\fake\..\Manual de produto.pdf");

        Assert.Equal($"{tenantId}/sources/{sourceId}/staging/Manual-de-produto.pdf", key);
    }

    [Fact]
    public void BuildStagingKey_UsesFallbackForEmptyFileName()
    {
        var key = SourceStorageKey.BuildStagingKey(Guid.NewGuid(), Guid.NewGuid(), "   ");

        Assert.EndsWith("/staging/upload", key, StringComparison.Ordinal);
    }

    [Fact]
    public void ToVerifiedAndQuarantineKey_ConvertOnlyStagingKeys()
    {
        var stagingKey = SourceStorageKey.BuildStagingKey(Guid.NewGuid(), Guid.NewGuid(), "manual.pdf");

        Assert.Contains("/verified/", SourceStorageKey.ToVerifiedKey(stagingKey), StringComparison.Ordinal);
        Assert.Contains("/quarantine/", SourceStorageKey.ToQuarantineKey(stagingKey), StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => SourceStorageKey.ToVerifiedKey("not/a/source/key"));
    }

    [Fact]
    public void IsVerifiedKey_ReturnsFalseForMalformedKeys()
    {
        Assert.False(SourceStorageKey.IsVerifiedKey("not/a/source/key"));
        Assert.False(SourceStorageKey.IsVerifiedKey(null));
    }
}
