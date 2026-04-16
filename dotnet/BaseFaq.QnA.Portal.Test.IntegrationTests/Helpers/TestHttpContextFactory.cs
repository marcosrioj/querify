using BaseFaq.Common.Infrastructure.Core.Attributes;
using Microsoft.AspNetCore.Http;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;

public static class TestHttpContextFactory
{
    public static HttpContext CreateWithTenantValidationSkipped()
    {
        var httpContext = new DefaultHttpContext();
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new SkipTenantAccessValidationAttribute()),
            "skip-tenant");
        httpContext.SetEndpoint(endpoint);
        return httpContext;
    }
}
