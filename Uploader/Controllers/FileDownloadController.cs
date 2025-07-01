using Microsoft.AspNetCore.Mvc;
using SimpleResults;
using Uploader.Api.Helpers;
using Uploader.Api.Models;
using Uploader.Api.Services;

namespace Uploader.Api.Controllers
{
    [ApiController]
    [ApiVersionNeutral]
    public class FileDownloadController : ControllerBase
    {
        private readonly IFileDownloadService _fileDownloadService;
        private readonly ILogger<FileDownloadController> _logger;

        public FileDownloadController(IFileDownloadService fileDownloadService, ILogger<FileDownloadController> logger)
        {
            _fileDownloadService = fileDownloadService;
            _logger = logger;
        }

        [HttpGet("{bucketName}/{path}")]
        public async Task<IResult> GetFile(string bucketName, string path)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("bucketName is not specified.");
                return Result.Invalid("Bad Request", ["bucketName is not specified."]).ToHttpResult();
            }

            if (string.IsNullOrEmpty(path))
            {
                _logger.LogWarning("No Path provided.");
                return Result.Invalid("Bad Request", ["No Path provided."]).ToHttpResult();
            }

            var result = await _fileDownloadService.GetFileAsync(bucketName, path);
            if (result.IsFailed)
                return Result.Failure(result.Message, result.Errors).ToHttpResult();

            return Result.File(result.Data).ToHttpResult();
        }

        [HttpGet("{bucketName}/{path}/byteArray")]
        [ServiceFilter(typeof(ApiKeyActionFilter))]
        public async Task<Result<ByteArrayFileContent>> GetFileAsByteArray(string bucketName, string path)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("bucketName is not specified.");
                return Result.Invalid("Bad Request", ["bucketName is not specified."]);
            }

            if (string.IsNullOrEmpty(path))
            {
                _logger.LogWarning("No Path provided.");
                return Result.Invalid("Bad Request", ["No Path provided."]);
            }

            var result = await _fileDownloadService.GetFileAsync(bucketName, path);
            if (result.IsFailed)
                return Result.Failure(result.Message, result.Errors);

            return Result.File(result.Data);
        }

        [HttpGet("{bucketName}/{path}/stream")]
        [ServiceFilter(typeof(ApiKeyActionFilter))]
        public async Task<IResult> GetFileAsStream(string bucketName, string path)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                _logger.LogWarning("bucketName is not specified.");
                return Result.Invalid("Bad Request", ["bucketName is not specified."]).ToHttpResult();
            }

            if (string.IsNullOrEmpty(path))
            {
                _logger.LogWarning("No Path provided.");
                return Result.Invalid("Bad Request", ["No Path provided."]).ToHttpResult();
            }

            var result = await _fileDownloadService.GetFileAsync(bucketName, path);
            if (result.IsFailed)
                return Result.Failure(result.Message, result.Errors).ToHttpResult();

            var memoryStream = new MemoryStream(result.Data.Content);
            return Result.File(new StreamFileContent(memoryStream)
            {
                ContentType = result.Data.ContentType,
                FileName = result.Data.FileName
            }).ToHttpResult();
        }

        [HttpGet("{bucketName}/{path}/metadata")]
        [ServiceFilter(typeof(ApiKeyActionFilter))]
        public async Task<Result<FileMetadataDto>> GetFileMetadata(string bucketName, string path)
        {
            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(path))
                return Result.Failure("Bad request", ["Bucket name and path are required."]);

            var result = await _fileDownloadService.GetFileMetadataAsync(bucketName, path);
            if (result.IsFailed)
                return Result.Failure(result.Message, result.Errors);

            return result.Data;
        }

        [HttpGet("{bucketName}/{objectName}/presignedUrl")]
        [ServiceFilter(typeof(ApiKeyActionFilter))]
        public async Task<Result<string>> GetPresignedUrl(string bucketName, string objectName, int expiresInMinutes)
        {
            var result = await _fileDownloadService.GetPresignedUrlAsync(bucketName, objectName, expiresInMinutes);
            if (result.IsFailed)
            {
                return Result.Failure(result.Message, result.Errors);
            }

            return result.Data;
        }

        [HttpGet("{bucketName}/{path}/partial")]
        [ServiceFilter(typeof(ApiKeyActionFilter))]
        public async Task<IResult> GetFilePartial(string bucketName, string path)
        {
            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(path))
                return Result.Failure("Bad Request", ["Bucket name and path are required."]).ToHttpResult();

            var result = await _fileDownloadService.GetFileAsync(bucketName, path);
            if (result.IsFailed)
                return Result.Failure(result.Message, result.Errors).ToHttpResult();

            var fileStream = new MemoryStream(result.Data.Content);
            return Results.File(fileStream, result.Data.ContentType, enableRangeProcessing: true);
        }

        [HttpGet("{bucketName}/list")]
        [ServiceFilter(typeof(ApiKeyActionFilter))]
        public async Task<Result<List<string>>> ListFiles(string bucketName, string prefix)
        {
            var listResult = await _fileDownloadService.GetListFilesAsync(bucketName, prefix);
            if (listResult.IsFailed)
                return Result.Failure(listResult.Message, listResult.Errors);

            return listResult.Data;
        }
    }
}
