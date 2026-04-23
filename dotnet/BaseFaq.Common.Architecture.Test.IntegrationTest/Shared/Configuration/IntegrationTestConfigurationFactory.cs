using Microsoft.Extensions.Configuration;

namespace BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Configuration;

public static class IntegrationTestConfigurationFactory
{
    public static IConfiguration Create()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();
    }
}
