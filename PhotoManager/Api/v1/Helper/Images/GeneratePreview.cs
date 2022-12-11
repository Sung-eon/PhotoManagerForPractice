namespace PhotoManager.Api.v1.Helper.Images;

using ImageMagick;

public static class GeneratePreview
{

    public static void CreateThumbnailFolder(string ThumbnailDirPath)
    {
        if (!Directory.Exists(ThumbnailDirPath))
        {
            Directory.CreateDirectory(ThumbnailDirPath);
        }
    }

    public static async Task ThumbnailWebP(string OriginalFilePath, string ThumbnailDirPath)
    {
        string NewThumbnailPath = Path.Combine(ThumbnailDirPath, "thumbnail.webp");

        if (!System.IO.File.Exists(NewThumbnailPath))
        {
            using (MagickImage image = new(OriginalFilePath))
            {
                int width = 800;
                int height = image.Height * (width / image.Width);
                image.Resize(width, height);

                await image.WriteAsync(NewThumbnailPath, MagickFormat.WebP);
            }
        }
    }

    public static async Task PreviewFiles(string OriginalFilePath, string ThumbnailDirPath)
    {
        //string NewPreviewPath_1x = Path.Combine(ThumbnailDirPath, "preview.webp");
        string NewPreviewPath_2x = Path.Combine(ThumbnailDirPath, "preview_2x.webp");
        string NewPreviewPath_4x = Path.Combine(ThumbnailDirPath, "preview_4x.webp");

        using (MagickImage image = new(OriginalFilePath))
        {
            /*
            if (!System.IO.File.Exists(NewPreviewPath_1x))
            {
                // Image Preview 1x size
                await image.WriteAsync(NewPreviewPath_1x, MagickFormat.WebP);
            }
            */

            image.Resize(image.Width / 2, image.Height / 2);

            if (!System.IO.File.Exists(NewPreviewPath_2x))
            {
                // Image Preview 0.5x size
                await image.WriteAsync(NewPreviewPath_2x, MagickFormat.WebP);
            }

            image.Resize(image.Width / 2, image.Height / 2);

            if (!System.IO.File.Exists(NewPreviewPath_4x))
            {
                // Image Preview 0.25x size
                await image.WriteAsync(NewPreviewPath_4x, MagickFormat.WebP);
            }

        }
    }
}