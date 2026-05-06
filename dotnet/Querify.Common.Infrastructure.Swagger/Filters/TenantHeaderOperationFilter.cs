using Querify.Common.Infrastructure.Core.Middleware;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Querify.Common.Infrastructure.Swagger.Filters;

public sealed class TenantHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();

        if (operation.Parameters.Any(parameter =>
                parameter.In == ParameterLocation.Header &&
                string.Equals(parameter.Name, TenantResolutionMiddleware.TenantHeaderName,
                    StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = TenantResolutionMiddleware.TenantHeaderName,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Tenant identifier resolved by middleware. Example: 11111111-1111-1111-1111-111111111111",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "uuid"
            }
        });
    }
}