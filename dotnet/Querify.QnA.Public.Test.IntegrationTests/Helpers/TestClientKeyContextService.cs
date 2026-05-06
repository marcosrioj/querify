using Querify.Common.Infrastructure.Core.Abstractions;

namespace Querify.QnA.Public.Test.IntegrationTests.Helpers;

public sealed class TestClientKeyContextService(string clientKey) : IClientKeyContextService
{
    public string GetRequiredClientKey()
    {
        return clientKey;
    }
}