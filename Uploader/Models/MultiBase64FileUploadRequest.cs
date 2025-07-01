namespace Uploader.Api.Models
{
    public class MultiBase64FileUploadRequest
    {
        public List<Base64FileUploadRequest> Files { get; set; }
    }
}
