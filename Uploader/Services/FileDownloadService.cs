using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using SimpleResults;
using Uploader.Api.Models;

namespace Uploader.Api.Services
{
    public interface IFileDownloadService
    {
        Task<Result<ByteArrayFileContent>> GetFileAsync(string bucketName, string objectName);
        Task<Result<FileMetadataDto>> GetFileMetadataAsync(string bucketName, string objectName);
        Task<Result<string>> GetPresignedUrlAsync(string bucketName, string objectName, int expiresInMinutes);
        Task<Result<List<string>>> GetListFilesAsync(string bucketName, string prefix);
    }

    public class FileDownloadService : IFileDownloadService
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<FileDownloadService> _logger;

        public FileDownloadService(IMinioClient minioClient,
            ILogger<FileDownloadService> logger)
        {
            _minioClient = minioClient;
            _logger = logger;
        }

        public async Task<Result<ByteArrayFileContent>> GetFileAsync(string bucketName, string objectName)
        {
            var decodedObjectName = Uri.UnescapeDataString(objectName);
            try
            {
                var memoryStream = new MemoryStream();
                var getObjectArgs = new GetObjectArgs()
                     .WithBucket(bucketName)
                     .WithObject(decodedObjectName)
                     .WithCallbackStream((stream) =>
                     {
                         stream.CopyTo(memoryStream);
                     });

                var result = await _minioClient.GetObjectAsync(getObjectArgs);
                return new ByteArrayFileContent(memoryStream.ToArray()) { ContentType = result.ContentType, FileName = Path.GetFileName(decodedObjectName) };
            }
            catch (ObjectNotFoundException ex)
            {
                _logger.LogWarning($"The specified path '{decodedObjectName}' in bucket '{bucketName}' was not found. {ex.Message}");
                return Result.Failure("Downloading File was failed.", [$"The specified path '{decodedObjectName}' in bucket '{bucketName}' was not found."]);
            }
            catch (Exception ex)
            {
                _logger.LogError($"internal server error. {ex.Message}");
                return Result.Failure("Downloading File was failed.", ["internal server error"]);
            }
        }

        public async Task<Result<FileMetadataDto>> GetFileMetadataAsync(string bucketName, string objectName)
        {
            var decodedObjectName = Uri.UnescapeDataString(objectName);
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(decodedObjectName);

                var statObjectResult = await _minioClient.StatObjectAsync(statObjectArgs);

                if (statObjectResult == null || statObjectResult.MetaData.Count == 0)
                {
                    return Result.Failure("No metadata found for the specified object", [$"No metadata found for the object {decodedObjectName}."]);
                }

                var metadata = new FileMetadataDto
                {
                    ObjectName = decodedObjectName,
                    BucketName = bucketName,
                    Size = statObjectResult.Size,
                    LastModified = statObjectResult.LastModified,
                    ContentType = statObjectResult.ContentType,
                    ETag = statObjectResult.ETag,
                    CustomMetadata = statObjectResult.MetaData
                };

                return metadata;
            }
            catch (MinioException ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving metadata for {decodedObjectName} in {bucketName}");
                return Result.Failure("Error occurred while retrieving metadata", [$"Error occurred while retrieving metadata for {decodedObjectName} in {bucketName}"]);
            }
        }

        public async Task<Result<string>> GetPresignedUrlAsync(string bucketName, string objectName, int expiresInMinutes)
        {
            var decodedObjectName = Uri.UnescapeDataString(objectName);
            try
            {
                var presignedGetObjectArgs = new PresignedGetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(decodedObjectName)
                    .WithExpiry((int)TimeSpan.FromMinutes(expiresInMinutes).TotalSeconds);

                var url = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
                return url;
            }
            catch (MinioException ex)
            {
                _logger.LogError(ex, $"Error occurred while generating presigned URL for {decodedObjectName} in {bucketName}");
                return Result.Failure("Failed to generate presigned URL.", [$"Error occurred while generating presigned URL for {decodedObjectName} in {bucketName}"]);
            }
        }

        public async Task<Result<List<string>>> GetListFilesAsync(string bucketName, string prefix)
        {
            try
            {
                var files = new List<string>();

                var listArgs = new ListObjectsArgs()
                    .WithBucket(bucketName)
                    .WithPrefix(prefix)
                    .WithRecursive(true);

                var tcs = new TaskCompletionSource<bool>();

                var observable = _minioClient.ListObjectsAsync(listArgs);
                observable.Subscribe(
                    item =>
                    {
                        files.Add(item.Key);
                    },
                    ex =>
                    {
                        tcs.TrySetException(ex);
                    },
                    () =>
                    {
                        tcs.TrySetResult(true);
                    }
                );

                await tcs.Task;

                return files;
            }
            catch (MinioException ex)
            {
                _logger.LogWarning(ex, $"Error listing files in {bucketName}");
                return Result.Failure("Error listing files", [$"Error listing files in {bucketName}"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error listing files in {bucketName}");
                return Result.Failure("Error listing files", [$"Error listing files in {bucketName}"]);
            }
        }
    }
}
