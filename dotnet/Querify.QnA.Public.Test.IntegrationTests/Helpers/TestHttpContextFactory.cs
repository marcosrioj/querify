using Querify.Common.Architecture.Test.IntegrationTest.Shared.Session;
using Querify.Common.Infrastructure.Core.Attributes;
using Microsoft.AspNetCore.Http;

namespace Querify.QnA.Public.Test.IntegrationTests.Helpers;

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
