using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Querify.Common.Infrastructure.Core.Options;

public class CorsOptions
{
    public const string Name = "CORS";

    public string AllowedOrigins { get; set; } = string.Empty;

    public bool EnableWebSocketCors { get; set; }

    [Required] public bool AllowAnyOrigins { get; set; }
}

public class CorsOptionsValidation(IConfiguration config) : IValidateOptions<CorsOptions>
{
    private CorsOptions? Config { get; } = config
        .GetSection(CorsOptions.Name)
        .Get<CorsOptions>();

    public ValidateOptionsResult Validate(string? name, CorsOptions options)
    {
        if (!options.AllowAnyOrigins)
        {
            var domains = options.AllowedOrigins.Split(';');

            foreach (var domain in domains)
            {
                if (string.IsNullOrEmpty(domain))
                {
                    return ValidateOptionsResult.Fail("Empty domain not allowed on CORS");
                }

                var uri = new Uri(domain);

                if (Uri.CheckHostName(uri.Host) == UriHostNameType.Unknown)
                {
                    return ValidateOptionsResult.Fail($"Invalid domain {domain} on CORS");
                }
            }
        }

        return ValidateOptionsResult.Success;
    }
}