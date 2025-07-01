namespace Uploader.Api.Models
{
    public class FilePathUploadRequest
    {
        public IFormFile File { get; set; }
        public string DestinationPath { get; set; }
    }
}
