using BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Session;
using BaseFaq.Common.Infrastructure.Core.Attributes;
using Microsoft.AspNetCore.Http;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;

public static class TestHttpContextFactory
{
    public static HttpContext CreateWithTenantValidationSkipped()
    {
        return IntegrationTestHttpContextFactory.CreateWithEndpointMetadata(
            "QnAPublicTest/1.0",
            "skip-tenant",
            new SkipTenantAccessValidationAttribute());
    }
}
