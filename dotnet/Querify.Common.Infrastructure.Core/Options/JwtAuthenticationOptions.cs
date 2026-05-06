using System.ComponentModel.DataAnnotations;

namespace Querify.Common.Infrastructure.Core.Options;

public class JwtAuthenticationOptions
{
    public const string Name = "JwtAuthentication";

    [Required] public string Authority { get; set; } = string.Empty;

    [Required] public string Audience { get; set; } = string.Empty;

    public bool RequireHttpsMetadata { get; set; } = false;

    public bool IncludeErrorDetails { get; set; } = false;
}