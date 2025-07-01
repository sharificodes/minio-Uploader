namespace Uploader.Api.Models
{
    public class MultiFilePathUploadRequest
    {
        public IFormFileCollection Files { get; set; }
        public string DestinationPath { get; set; }
    }
}
