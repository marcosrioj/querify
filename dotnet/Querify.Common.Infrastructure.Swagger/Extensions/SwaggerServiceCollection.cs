using Querify.Common.Infrastructure.Core.Middleware;
using Querify.Common.Infrastructure.Swagger.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Querify.Common.Infrastructure.Swagger.Extensions;

public static class SwaggerServiceCollection
{
    public static IServiceCollection LoadSwaggerOptions(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddOptions<SwaggerOptions>()
            .Bind(configuration.GetSection(SwaggerOptions.Name))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }


    public static void AddSwagger(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.LoadSwaggerOptions(configuration);

        var options = configuration.GetSection("SwaggerOptions").Get<SwaggerOptions>();

        services.AddSwaggerGen(c =>
        {
            c.UseOneOfForPolymorphism();
            c.EnableAnnotations();
            c.CustomSchemaIds(type => type.FullName);

            var version = options?.Version ?? "v1";
            var title = options?.Title ?? "Swagger API";
            var contextHeaderName = ResolveContextHeaderName(options);
            var apiDescription = ResolveApiDescription(options, contextHeaderName);

            c.SwaggerDoc(version, new OpenApiInfo
            {
                Title = title,
                Version = version,
                Description = apiDescription
            });

            if (options?.EnableTenantHeader ?? true)
            {
                var tenantScheme = CreateContextHeaderSecurityScheme(options, contextHeaderName);

                c.AddSecurityDefinition("ContextHeader", tenantScheme);
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("ContextHeader", document)] = []
                });
            }
        });
    }

    public static void AddSwaggerWithAuth(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.LoadSwaggerOptions(configuration);

        var options = configuration.GetSection("SwaggerOptions").Get<SwaggerOptions>();

        services.AddSwaggerGen(c =>
        {
            c.UseOneOfForPolymorphism();
            c.EnableAnnotations();
            c.CustomSchemaIds(type => type.FullName);

            var version = options?.Version ?? "v1";
            var title = options?.Title ?? "Swagger API";
            var contextHeaderName = ResolveContextHeaderName(options);
            var apiDescription = ResolveApiDescription(options, contextHeaderName);

            c.SwaggerDoc(version, new OpenApiInfo
            {
                Title = title,
                Version = version,
                Description = apiDescription
            });

            var scheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,

                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(options!.swaggerAuth!.AuthorizeEndpoint),
                        TokenUrl = new Uri(options.swaggerAuth.TokenEndpoint)
                    }
                },
                Description = "Server OpenId Security Scheme"
            };


            if (options.swaggerAuth.EnableClientCredentials)
            {
                scheme.Flows.ClientCredentials = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(options.swaggerAuth.AuthorizeEndpoint),
                    TokenUrl = new Uri(options.swaggerAuth.TokenEndpoint)
                };
            }

            c.AddSecurityDefinition("OAuth2", scheme);

            if (options?.EnableTenantHeader ?? true)
            {
                var tenantScheme = CreateContextHeaderSecurityScheme(options, contextHeaderName);

                c.AddSecurityDefinition("ContextHeader", tenantScheme);
                c.AddSecurityRequirement(document =>
                {
                    var requirement = new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("OAuth2", document)] = []
                    };

                    requirement[new OpenApiSecuritySchemeReference("ContextHeader", document)] = [];
                    return requirement;
                });
            }
            else
            {
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("OAuth2", document)] = []
                });
            }
        });
    }

    public static IApplicationBuilder UseSwaggerUIWithAuth(this IApplicationBuilder app)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

        app.UseSwaggerUI(options =>
        {
            var swaggerAuth = configuration.GetSection("SwaggerOptions")
                .Get<SwaggerOptions>()?.swaggerAuth;

            if (!string.IsNullOrWhiteSpace(swaggerAuth?.ClientId))
            {
                options.OAuthClientId(swaggerAuth.ClientId);
            }

            if (!string.IsNullOrWhiteSpace(swaggerAuth?.ClientSecret))
            {
                options.OAuthClientSecret(swaggerAuth.ClientSecret);
            }

            if (!string.IsNullOrWhiteSpace(swaggerAuth?.Audience))
            {
                options.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
                {
                    ["audience"] = swaggerAuth.Audience
                });
            }

            options.EnablePersistAuthorization();
        });

        return app;
    }

    private static string ResolveContextHeaderName(SwaggerOptions? options)
    {
        if (!string.IsNullOrWhiteSpace(options?.ContextHeaderName))
        {
            return options.ContextHeaderName;
        }

        return TenantResolutionMiddleware.TenantHeaderName;
    }

    private static string ResolveApiDescription(SwaggerOptions? options, string contextHeaderName)
    {
        if (string.Equals(contextHeaderName, ClientKeyResolutionMiddleware.ClientKeyHeaderName,
                StringComparison.OrdinalIgnoreCase))
        {
            return $"Client context is provided via {contextHeaderName} header and resolved by middleware.";
        }

        return $"Tenant context is provided via {contextHeaderName} header and resolved by middleware.";
    }

    private static OpenApiSecurityScheme CreateContextHeaderSecurityScheme(
        SwaggerOptions? options,
        string contextHeaderName)
    {
        var schemeDescription = options?.ContextHeaderDescription;
        if (string.IsNullOrWhiteSpace(schemeDescription))
        {
            schemeDescription = $"Set '{contextHeaderName}' once in the Authorize dialog.";
        }

        return new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = contextHeaderName,
            In = ParameterLocation.Header,
            Description = schemeDescription
        };
    }
}