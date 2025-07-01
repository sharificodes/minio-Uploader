using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace Uploader.Api.Configs
{
    public static class ConfigSwaggerVersion
    {
        public static void SwaggerConfiguration(this IServiceCollection serviceCollection)
        {
            serviceCollection.ConfigureOptions<ConfigureSwaggerOptions>();
            serviceCollection.AddApiVersioning(delegate (ApiVersioningOptions options)
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
            });
            serviceCollection.AddVersionedApiExplorer(delegate (ApiExplorerOptions setup)
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });
        }
    }
}
