using System.ComponentModel.DataAnnotations;
using Querify.Common.Infrastructure.Core.Middleware;

namespace Querify.Common.Infrastructure.Swagger.Options;

public class SwaggerOptions
{
    public const string Name = "SwaggerOptions";
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string Version { get; set; } = string.Empty;
    public bool EnableTenantHeader { get; set; } = true;
    public string ContextHeaderName { get; set; } = TenantResolutionMiddleware.TenantHeaderName;
    public string? ContextHeaderDescription { get; set; }
    public SwaggerAuthOptions? swaggerAuth { get; set; }
}

public class SwaggerAuthOptions
{
    public bool EnableClientCredentials { get; set; }

    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Audience { get; set; }

    [Required] public string AuthorizeEndpoint { get; set; } = string.Empty;
    [Required] public string TokenEndpoint { get; set; } = string.Empty;
}