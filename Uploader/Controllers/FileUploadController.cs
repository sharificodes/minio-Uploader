using Microsoft.AspNetCore.Mvc;
using SimpleResults;
using System;
using Uploader.Api.Helpers;
using Uploader.Api.Models;
using Uploader.Api.Services;
using Uploader.Enums;

namespace Uploader.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ServiceFilter(typeof(ApiKeyActionFilter))]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(IFileUploadService fileUploadService, ILogger<FileUploadController> logger)
        {
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        [HttpPost("{bucketName}")]
        public async Task<Result<string>> UploadFromInput(string bucketName, [FromForm] IFormFile file)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("bucketName is not specified.");
                return Result.Invalid("Bad request", ["bucketName is not specified."]);
            }

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                return Result.Invalid("Bad request", ["No file uploaded."]);
            }

            try
            {
                var result = await _fileUploadService.UploadFileAsync(bucketName, file);
                if (result.IsFailed)
                    return Result.Failure(result.Message, result.Errors);

                _logger.LogInformation("File uploaded successfully: {FileUrl}", result.Data);
                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file.");
                return Result.Failure("Error uploading file.");
            }
        }

        [HttpPost("{bucketName}/to-path")]
        public async Task<Result<string>> UploadFileToPath(string bucketName, [FromForm] FilePathUploadRequest request)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("bucketName is not specified.");
                return Result.Invalid("Bad request", ["bucketName is not specified."]);
            }

            if (request.File == null || request.File.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                return Result.Invalid("Bad request", ["No file uploaded."]);
            }

            if (string.IsNullOrEmpty(request.DestinationPath))
            {
                _logger.LogWarning("Destination path is not specified.");
                return Result.Invalid("Bad request", ["Destination path is not specified."]);
            }

            try
            {
                var result = await _fileUploadService.UploadFileAsync(bucketName, request.File, request.DestinationPath);
                if (result.IsFailed)
                    return Result.Failure(result.Message, result.Errors);

                _logger.LogInformation("File uploaded to path {DestinationPath} successfully: {FileUrl}", request.DestinationPath, result.Data);
                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to path {DestinationPath}.", request.DestinationPath);
                return Result.Failure($"Error uploading file to path {request.DestinationPath}.");
            }
        }

        [HttpPost("multiple/{bucketName}")]
        public async Task<List<Result<string>>> UploadMultipleFiles(string bucketName, [FromForm] IFormFileCollection files)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("Bucket name is not specified.");
                return new List<Result<string>> { Result.Invalid("Bad request", ["Bucket name is not specified."]) };
            }

            if (files == null || files.Count == 0)
            {
                _logger.LogWarning("No files uploaded.");
                return new List<Result<string>> { Result.Invalid("Bad request", ["No files uploaded."]) };
            }

            var results = new List<Result<string>>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {

                    var result = await _fileUploadService.UploadFileAsync(bucketName, file);
                    if (result.IsFailed)
                    {
                        results.Add(Result.Failure(result.Message, result.Errors));
                        continue;
                    }

                    results.Add(result.Data);
                }
            }

            _logger.LogInformation("File uploaded successfully");
            return results;
        }

        [HttpPost("multiple/{bucketName}/to-path")]
        public async Task<List<Result<string>>> UploadFilesToPath(string bucketName, [FromForm] MultiFilePathUploadRequest request)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("bucketName is not specified.");
                return [Result.Invalid("Bad request", ["bucketName is not specified."])];
            }

            if (request.Files == null || request.Files.Count == 0)
            {
                _logger.LogWarning("No files uploaded.");
                return [Result.Invalid("Bad request", ["No files uploaded."])];
            }

            if (string.IsNullOrEmpty(request.DestinationPath))
            {
                _logger.LogWarning("Destination path is not specified.");
                return [Result.Invalid("Bad request", ["Destination path is not specified."])];
            }

            var results = new List<Result<string>>();
            foreach (var file in request.Files)
            {
                if (file.Length > 0)
                {
                    var result = await _fileUploadService.UploadFileAsync(bucketName, file, request.DestinationPath);
                    if (result.IsFailed)
                    {
                        results.Add(Result.Failure(result.Message, result.Errors));
                        continue;
                    }

                    results.Add(result.Data);
                }
            }

            _logger.LogInformation("File uploaded to path {DestinationPath} successfully", request.DestinationPath);

            return results;
        }

        [HttpPost("from-url/{bucketName}")]
        public async Task<Result<string>> UploadFromUrl(string bucketName, [FromBody] string url)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("bucketName is not specified.");
                return Result.Invalid("Bad request", ["bucketName is not specified."]);
            }

            if (string.IsNullOrEmpty(url))
            {
                _logger.LogWarning("No URL provided.");
                return Result.Invalid("Bad request", ["No URL provided."]);
            }

            try
            {
                var result = await _fileUploadService.UploadFileFromUrlAsync(bucketName, url);
                if (result.IsFailed)
                    return Result.Failure(result.Message, result.Errors);

                _logger.LogInformation("File from {Url} uploaded successfully: {FileUrl}", url, result.Data);
                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file from URL.");
                return Result.Failure("Error uploading file from URL.");
            }
        }

        [HttpPost("from-url/{bucketName}/to-path")]
        public async Task<Result<string>> UploadUrlToPath(string bucketName, [FromBody] UrlPathUploadRequest request)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("bucketName is not specified.");
                return Result.Invalid("Bad request", ["bucketName is not specified."]);
            }

            if (string.IsNullOrEmpty(request.Url))
            {
                _logger.LogWarning("The URL is empty.");
                return Result.Invalid("Bad request", ["The URL is empty."]);
            }

            if (string.IsNullOrEmpty(request.DestinationPath))
            {
                _logger.LogWarning("Destination path is not specified.");
                return Result.Invalid("Bad request", ["Destination path is not specified."]);
            }

            try
            {
                var result = await _fileUploadService.UploadFileFromUrlAsync(bucketName, request.Url, request.DestinationPath);
                if (result.IsFailed)
                    return Result.Failure(result.Message, result.Errors);

                _logger.LogInformation("File from {Url} to path {DestinationPath} uploaded successfully: {FileUrl}", request.Url, request.DestinationPath, result.Data);
                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file from URL {Url} to path {DestinationPath}.", request.Url, request.DestinationPath);
                return Result.Failure($"Error uploading file from URL {request.Url} to path {request.DestinationPath}");
            }
        }

        [HttpPost("from-urls/{bucketName}")]
        public async Task<List<Result<string>>> UploadFromUrls(string bucketName, [FromBody] List<string> urls)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("bucketName is not specified.");
                return [Result.Invalid("Bad request", ["bucketName is not specified."])];
            }

            if (urls == null || urls.Count == 0)
            {
                _logger.LogWarning("No URLs provided.");
                return [Result.Invalid("Bad request", ["No URLs provided."])];
            }

            var results = new List<Result<string>>();

            foreach (var url in urls)
            {
                if (!string.IsNullOrEmpty(url))
                {
                    var result = await _fileUploadService.UploadFileFromUrlAsync(bucketName, url);
                    if (result.IsFailed)
                    {
                        results.Add(Result.Failure(result.Message, result.Errors));
                    }

                    results.Add(result.Data);
                }
            }

            _logger.LogInformation("Files uploaded successfully.");
            return results;
        }

        [HttpPost("from-urls/{bucketName}/to-path")]
        public async Task<List<Result<string>>> UploadUrlsToPath(string bucketName, [FromBody] UrlsPathUploadRequest request)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("bucketName is not specified.");
                return [Result.Invalid("Bad request", ["bucketName is not specified."])];
            }

            if (request.Urls == null || request.Urls.Count == 0)
            {
                _logger.LogWarning("No URLs provided.");
                return [Result.Invalid("Bad request", ["No URLs provided."])];
            }

            if (string.IsNullOrEmpty(request.DestinationPath))
            {
                _logger.LogWarning("Destination path is not specified.");
                return [Result.Invalid("Bad request", ["Destination path is not specified."])];
            }

            var results = new List<Result<string>>();

            foreach (var url in request.Urls)
            {
                if (!string.IsNullOrEmpty(url))
                {
                    var result = await _fileUploadService.UploadFileFromUrlAsync(bucketName, url, request.DestinationPath);
                    if (result.IsFailed)
                    {
                        results.Add(Result.Failure(result.Message, result.Errors));
                    }

                    results.Add(result.Data);
                }
            }

            _logger.LogInformation("Files to path {DestinationPath} uploaded successfully", request.DestinationPath);

            return results;
        }

        [HttpPost("from-base64/{bucketName}")]
        public async Task<Result<string>> UploadFromBase64(string bucketName, [FromBody] Base64FileUploadRequest request)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("bucketName is not specified.");
                return Result.Invalid("Bad request", ["bucketName is not specified."]);
            }

            if (string.IsNullOrEmpty(request.FileBase64AsString))
            {
                _logger.LogWarning("No file provided.");
                return Result.Invalid("Bad request", ["No file provided."]);
            }

            var result = await _fileUploadService.UploadFileFromBase64Async(bucketName, request.FileBase64AsString, request.FileExtension.ToExtension());
            if (result.IsFailed)
                return Result.Failure(result.Message, result.Errors);

            _logger.LogInformation("Base64 file uploaded successfully: {FileUrl}", result.Data);
            return result.Data;
        }

        [HttpPost("from-multi-base64/{bucketName}")]
        public async Task<List<Result<string>>> UploadMultipleBase64Files(string bucketName, [FromBody] MultiBase64FileUploadRequest request)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("bucketName is not specified.");
                return [Result.Invalid("Bad request", ["bucketName is not specified."])];
            }


            if (request.Files == null || request.Files.Count == 0)
            {
                _logger.LogWarning("No files provided.");
                return [Result.Invalid("Bad request", ["No files provided."])];
            }

            var results = new List<Result<string>>();

            foreach (var fileRequest in request.Files)
            {
                if (!string.IsNullOrEmpty(fileRequest.FileBase64AsString))
                {
                    var result = await _fileUploadService.UploadFileFromBase64Async(bucketName, fileRequest.FileBase64AsString, fileRequest.FileExtension.ToExtension());
                    if (result.IsFailed)
                    {
                        results.Add(Result.Failure(result.Message, result.Errors));
                    }

                    results.Add(result.Data);
                }
            }

            _logger.LogInformation("files uploaded successfully");

            return results;
        }
    }
}
