using Uploader.Enums;

namespace Uploader.Api.Models
{
    public class Base64FileUploadRequest
    {
        public string FileBase64AsString { get; set; }
        public FileExtensionType FileExtension { get; set; }
    }
}
