namespace PhotoManager.Api.v1.Helper.Images;


public static class CheckExtensions
{
    public static readonly string[] HeicFormats = new string[] { ".heic", ".heif" };
    public static readonly string[] RAWFormats = new string[] { ".arw" };
    public static readonly IEnumerable<string> ImageExts = new string[] {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    }.Union(RAWFormats)
     .Union(HeicFormats);


    public static bool IsImage(string path)
    {
        return ImageExts.Contains(Path.GetExtension(path).ToLower());
    }

    public static bool IsHEIC(string path)
    {
        return HeicFormats.Contains(Path.GetExtension(path).ToLower());
    }

    public static bool IsRAW(string path)
    {
        return RAWFormats.Contains(Path.GetExtension(path).ToLower());
    }
}


