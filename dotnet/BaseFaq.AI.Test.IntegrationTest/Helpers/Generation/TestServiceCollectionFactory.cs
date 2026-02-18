using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.AI.Test.IntegrationTest.Helpers.Generation;

public static class TestServiceCollectionFactory
{
    public static IServiceCollection Create() => new ServiceCollection();
}