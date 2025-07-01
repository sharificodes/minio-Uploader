
namespace Uploader.Enums;

public enum FileExtensionType
{
    Bin,
    Jpg,
    Png,
    Pdf,
    Txt,
    Zip,
    Mp3,
    Wav,
    Mp4,
    Avi,
    Mov,
    Mkv
}

public static class FileExtension
{
    private static readonly Dictionary<FileExtensionType, string> ExtensionToString = new()
    {
        { FileExtensionType.Bin, ".bin" },
        { FileExtensionType.Jpg, ".jpg" },
        { FileExtensionType.Png, ".png" },
        { FileExtensionType.Pdf, ".pdf" },
        { FileExtensionType.Txt, ".txt" },
        { FileExtensionType.Zip, ".zip" },
        { FileExtensionType.Mp3, ".mp3" },
        { FileExtensionType.Wav, ".wav" },
        { FileExtensionType.Mp4, ".mp4" },
        { FileExtensionType.Avi, ".avi" },
        { FileExtensionType.Mov, ".mov" },
        { FileExtensionType.Mkv, ".mkv" }
    };

    public static string ToExtension(this FileExtensionType fileExtensionType)
    {
        return ExtensionToString.TryGetValue(fileExtensionType, out var ext) ? ext : ExtensionToString[FileExtensionType.Bin];
    }
}
