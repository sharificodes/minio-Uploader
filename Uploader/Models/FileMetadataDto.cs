namespace Uploader.Api.Models
{
    public class FileMetadataDto
    {
        public string ObjectName { get; set; }
        public string BucketName { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string ContentType { get; set; }
        public string ETag { get; set; }
        public Dictionary<string, string> CustomMetadata { get; set; }
    }
}
