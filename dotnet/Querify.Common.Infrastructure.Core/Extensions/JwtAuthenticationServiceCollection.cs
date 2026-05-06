using System.IdentityModel.Tokens.Jwt;
using Querify.Common.Infrastructure.Core.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;

namespace Querify.Common.Infrastructure.Core.Extensions;

public static class JwtAuthenticationServiceCollection
{
    public static IServiceCollection AddDefaultAuthentication(this IServiceCollection services,
        ConfigurationManager configuration,
        string signalRHubUrl = "")
    {
        var jwtAuthenticationOptions =
            configuration.GetRequiredSection(JwtAuthenticationOptions.Name).Get<JwtAuthenticationOptions>();


        IdentityModelEventSource.ShowPII = jwtAuthenticationOptions!.IncludeErrorDetails;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.Authority = jwtAuthenticationOptions.Authority;
            o.Audience = jwtAuthenticationOptions.Audience;
            o.RequireHttpsMetadata = jwtAuthenticationOptions.RequireHttpsMetadata;
            o.IncludeErrorDetails = jwtAuthenticationOptions.IncludeErrorDetails;

            // Instruct the handler not to perform claims mapping
            var jwtHandler = new JwtSecurityTokenHandler
            {
                MapInboundClaims = false
            };
            o.TokenHandlers.Clear();
            o.TokenHandlers.Add(jwtHandler);

            //Set proper claimtypes for JWT
            o.TokenValidationParameters.RoleClaimType = "role";
            o.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.GivenName;
            if (jwtAuthenticationOptions.Audience.StartsWith("api://", StringComparison.OrdinalIgnoreCase))
            {
                var audienceId = jwtAuthenticationOptions.Audience.Substring("api://".Length);
                o.TokenValidationParameters.ValidAudiences = new[]
                {
                    jwtAuthenticationOptions.Audience,
                    audienceId
                };
            }

            o.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    // If the request is for our hub...
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        !string.IsNullOrEmpty(signalRHubUrl) && path.StartsWithSegments(signalRHubUrl))
                    {
                        // Read the token out of the query string
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });
        return services;
    }
}