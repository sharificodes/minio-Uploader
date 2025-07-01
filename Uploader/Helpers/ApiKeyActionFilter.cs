using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Uploader.Api.Helpers
{
    public class ApiKeyActionFilter : ActionFilterAttribute
    {
        public bool Optional { get; set; }

        public ApiKeyActionFilter(bool optional = false)
        {
            Optional = optional;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var appSettings = context.HttpContext.RequestServices.GetService<IOptions<AppSettings>>();

            appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));

            if (!context.HttpContext.Request.Headers.TryGetValue("X-API-KEY", out var apiKey))
            {
                context.Result = new BadRequestObjectResult("API key missing");
                return;
            }

            if (apiKey != appSettings?.Value?.ApiKey)
            {
                context.Result = new BadRequestObjectResult("Invalid API key");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
