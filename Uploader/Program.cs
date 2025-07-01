using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Minio;
using Minio.AspNetCore;
using Serilog;
using Uploader.Api;
using Uploader.Api.Configs;
using Uploader.Api.Helpers;
using Uploader.Api.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);


builder.Services.Configure<MinioStorage>(builder.Configuration.GetSection(MinioStorage.SectionName));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(AppSettings.SectionName));

var endpoint = builder.Configuration[$"{MinioStorage.SectionName}:{nameof(MinioStorage.Endpoint)}"];
var accessKey = builder.Configuration[$"{MinioStorage.SectionName}:{nameof(MinioStorage.AccessKey)}"];
var secretKey = builder.Configuration[$"{MinioStorage.SectionName}:{nameof(MinioStorage.SecretKey)}"];

builder.Services.AddMinio(options =>
{
    options.Endpoint = endpoint;
    options.AccessKey = accessKey;
    options.SecretKey = secretKey;
    options.ConfigureClient(client =>
    {
        client.WithTimeout(1000);
    });
});

builder.Services.AddTransient<ApiKeyActionFilter>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IFileDownloadService, FileDownloadService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.SwaggerConfiguration();

var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwagger(options => options.RouteTemplate = "swagger/{documentName}/swagger.json");
app.UseSwaggerUI(o =>
{
    foreach (var description in provider.ApiVersionDescriptions)
    {
        o.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
            $"{description.GroupName.ToUpper()}");
    }
});
app.UseRouting();
app.MapControllers();
app.Run();


