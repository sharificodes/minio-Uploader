using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Uploader.Api.Helpers;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasApiKeyActionFilter = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<ServiceFilterAttribute>().Any(f => f.ServiceType == typeof(ApiKeyActionFilter))
                                    || context.MethodInfo.GetCustomAttributes(true).OfType<ServiceFilterAttribute>().Any(f => f.ServiceType == typeof(ApiKeyActionFilter))
                                    || context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<TypeFilterAttribute>().Any(f => f.ImplementationType == typeof(ApiKeyActionFilter))
                                    || context.MethodInfo.GetCustomAttributes(true).OfType<TypeFilterAttribute>().Any(f => f.ImplementationType == typeof(ApiKeyActionFilter));

        if (hasApiKeyActionFilter)
        {
            var apiKeyScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            };

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [apiKeyScheme] = new List<string>()
                }
            };
        }
    }
}
