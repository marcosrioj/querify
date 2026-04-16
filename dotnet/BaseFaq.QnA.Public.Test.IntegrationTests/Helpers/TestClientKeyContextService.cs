using BaseFaq.Common.Infrastructure.Core.Abstractions;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;

public sealed class TestClientKeyContextService(string clientKey) : IClientKeyContextService
{
    public string GetRequiredClientKey() => clientKey;
}
