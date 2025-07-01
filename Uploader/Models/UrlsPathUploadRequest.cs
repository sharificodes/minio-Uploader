namespace Uploader.Api.Models
{
    public class UrlsPathUploadRequest
    {
        public List<string> Urls { get; set; }
        public string DestinationPath { get; set; }
    }
}
