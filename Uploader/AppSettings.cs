namespace Uploader.Api
{
    public class AppSettings
    {
        public static string SectionName => nameof(AppSettings);

        public string ApiKey { get; set; }
    }

    public class MinioStorage
    {
        public static string SectionName => nameof(MinioStorage);

        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BaseUrl { get; set; }
    }
}
