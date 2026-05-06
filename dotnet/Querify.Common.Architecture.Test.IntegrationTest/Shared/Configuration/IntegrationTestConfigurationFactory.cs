using Microsoft.Extensions.Configuration;

namespace Querify.Common.Architecture.Test.IntegrationTest.Shared.Configuration;

public static class IntegrationTestConfigurationFactory
{
    public static IConfiguration Create()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();
    }
}
