using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using SimpleResults;

namespace Uploader.Api.Services
{
    public interface IFileUploadService
    {
        Task<Result<string>> UploadFileAsync(string bucketName, IFormFile file, string destinationPath = "");
        Task<Result<string>> UploadFileFromUrlAsync(string bucketName, string url, string destinationPath = "");
        Task<Result<string>> UploadFileFromBase64Async(string bucketName, string base64String, string extension, string destinationPath = "");
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IMinioClient _minioClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FileUploadService> _logger;
        private readonly string _baseUrl;

        public FileUploadService(IMinioClient minioClient,
            IHttpClientFactory httpClientFactory,
            ILogger<FileUploadService> logger,
            IOptions<MinioStorage> options)
        {
            _minioClient = minioClient;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _baseUrl = options.Value.BaseUrl;
        }

        public async Task<Result<string>> UploadFileAsync(string bucketName, IFormFile file, string destinationPath = "")
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var objectName = Path.Combine(destinationPath, fileName).Replace("\\", "/").ToLower();

            using var stream = file.OpenReadStream();
            var result = await UploadFileAsync(bucketName, Uri.UnescapeDataString(objectName), stream, file.ContentType);
            if (result.IsFailed)
                return Result.Failure(result.Message, result.Errors);

            return result.Data;
        }

        public async Task<Result<string>> UploadFileFromUrlAsync(string bucketName, string url, string destinationPath = "")
        {
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                Result.Failure("Downloading File was failed.", [$"Failed to download file from url {url}."]);

            var fileNameWithExtension = Path.GetFileName(new Uri(url).AbsolutePath);
            var fileExtension = Path.GetExtension(fileNameWithExtension);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var objectName = Path.Combine(destinationPath, fileName).Replace("\\", "/").ToLower();

            var fileContent = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            using var stream = new MemoryStream(fileContent);
            var result = await UploadFileAsync(bucketName, Uri.UnescapeDataString(objectName), stream, contentType);
            if (result.IsFailed)
                return Result.Failure(result.Message, result.Errors);

            return result.Data;
        }

        public async Task<Result<string>> UploadFileFromBase64Async(string bucketName, string base64String, string extension, string destinationPath = "")
        {
            byte[] fileContent;
            try
            {
                fileContent = Convert.FromBase64String(base64String);
            }
            catch (FormatException ex)
            {
                _logger.LogWarning($"Invalid Base64 string provided. {ex}");
                return Result.Failure("Uploading File is failed.", ["Invalid Base64 string provided."]);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal server error. {ex.Message}");
                return Result.Failure("Uploading File was failed.", ["Internal server error."]);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var objectName = Path.Combine(destinationPath, fileName).Replace("\\", "/").ToLower();

            using var stream = new MemoryStream(fileContent);
            var result = await UploadFileAsync(bucketName, Uri.UnescapeDataString(objectName), stream, "application/octet-stream");
            if (result.IsFailed)
                return Result.Failure(result.Message, result.Errors);

            return result.Data;
        }

        private async Task<Result<string>> UploadFileAsync(string bucketName, string objectName, Stream stream, string contentType)
        {
            try
            {
                var beArgs = new BucketExistsArgs().WithBucket(bucketName);
                bool found = await _minioClient.BucketExistsAsync(beArgs);
                if (!found)
                    return Result.Failure("Uploading File was failed.", [$"Bucket '{bucketName}' does not exist."]);

                var result = await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType));

                return $"{_baseUrl}/{result.ObjectName}".ToLower();
            }
            catch (AuthorizationException ex)
            {
                _logger.LogWarning($"Invalid access or secret key. {ex.Message}");
                return Result.Failure("Uploading File was failed.", ["Invalid access or secret key."]);
            }
            catch (InvalidBucketNameException ex)
            {
                _logger.LogWarning($"Invalid bucket name: '{bucketName}'. {ex.Message}");
                return Result.Failure("Uploading File was failed.", [$"Invalid bucket name: '{bucketName}'."]);
            }
            catch (InvalidObjectNameException ex)
            {
                _logger.LogWarning($"Invalid object name: '{objectName}'. {ex.Message}");
                return Result.Failure("Uploading File was failed.", [$"Invalid object name: '{objectName}'."]);
            }
            catch (BucketNotFoundException ex)
            {
                _logger.LogWarning($"Bucket '{bucketName}' does not exist. {ex.Message}");
                return Result.Failure("Uploading File was failed.", [$"Bucket '{bucketName}' does not exist."]);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning($"The file to upload was not found. {ex.Message}");
                return Result.Failure("Uploading File was failed.", ["The file to upload was not found."]);
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogWarning($"The file stream has been disposed. {ex.Message}");
                return Result.Failure("Uploading File was failed.", ["The file stream has been disposed."]);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning($"The file stream cannot be read from. {ex.Message}");
                return Result.Failure("Uploading File was failed.", ["The file stream cannot be read from."]);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"The file stream is currently in a read operation. {ex.Message}");
                return Result.Failure("Uploading File was failed.", ["The file stream is currently in a read operation."]);
            }
            catch (AccessDeniedException ex)
            {
                _logger.LogWarning($"Access is denied for the encrypted PUT operation due to an incorrect key. {ex.Message}");
                return Result.Failure("Uploading File was failed.", ["Access is denied for the encrypted PUT operation due to an incorrect key."]);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal server error. {ex.Message}");
                return Result.Failure("Uploading File was failed.", ["Internal server error."]);
            }
        }
    }
}
