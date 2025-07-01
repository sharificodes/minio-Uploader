using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Uploader.Api.Configs
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (ApiVersionDescription apiVersionDescription in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(apiVersionDescription.GroupName, CreateVersionInfo(apiVersionDescription));
            }

            ConfigureSecurityRequirement(options);
            ConfigureMappingType(options);
        }

        private static void ConfigureMappingType(SwaggerGenOptions options)
        {
            options.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date"
            });
            options.MapType<TimeOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "time",
                Example = OpenApiAnyFactory.CreateFromJson("\"13:45:42.0000000\"")
            });

        }

        private static void ConfigureSecurityRequirement(SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "ApiKey must appear in header",
                Type = SecuritySchemeType.ApiKey,
                Name = "X-API-KEY",
                In = ParameterLocation.Header,
                Scheme = "ApiKeyScheme"
            });
 
            options.OperationFilter<SecurityRequirementsOperationFilter>();
        }

        private OpenApiInfo CreateVersionInfo(ApiVersionDescription desc)
        {
            OpenApiInfo openApiInfo = new OpenApiInfo
            {
                Title = Assembly.GetEntryAssembly().GetName().Name,
                Version = desc.ApiVersion.ToString()
            };
            if (desc.IsDeprecated)
            {
                openApiInfo.Description += " This API version has been deprecated. Please use one of the new APIs available from the explorer.";
            }

            return openApiInfo;
        }
    }
}
