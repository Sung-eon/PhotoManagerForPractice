namespace PhotoManager.Api.v1.Helper.Images;

using Microsoft.AspNetCore.Components.Forms;
using PhotoManager.Data;

public static class SaveNewPhoto
{
    private static string GetRootPath()
    {
        return Environment.GetEnvironmentVariable("PHOTO_ROOT_DIR");
    }

    private static string GetThumbnailPath()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "thumbnail");
    }

    private static string CreateNewDirectory(string manufacturer, string model, DateTime? createdTime)
    {

        string relFileDirPath;

        if (createdTime is null)
        {
            relFileDirPath = Path.Combine("PhotoLibrary", manufacturer, model, "unknown_datetime");
        }
        else
        {
            string ShortDateString = ((DateTime)createdTime).ToString("yyyy-MM-dd");
            relFileDirPath = Path.Combine("PhotoLibrary", manufacturer, model, ShortDateString);
        }

        string absFileDirPath = Path.Combine(GetRootPath(), relFileDirPath);

        if (!Directory.Exists(absFileDirPath))
        {
            Directory.CreateDirectory(absFileDirPath);
        }

        return absFileDirPath;
    }

    ///<summary>This method insert the metadata of a photo while copying a photo from the local device</summary>
    public static async Task<Photo> GetPhoto(IBrowserFile browserFile)
    {
        long maxFileSize = 50 * 1024 * 1024;

        using MemoryStream ms = new();

        using Stream BrowserFileStream = browserFile.OpenReadStream(maxFileSize);
        await BrowserFileStream.CopyToAsync(ms);
        ms.Seek(0, SeekOrigin.Begin);

        var metaData = ReadMetadata.ExtractData(ms);

        string manufacturer = metaData.GetValueOrDefault("maker", "unknown");
        string model = metaData.GetValueOrDefault("model", "unknown");
        DateTime? createdTime = metaData.GetValueOrDefault("original_datetime", null);

        string FileDirPath = CreateNewDirectory(manufacturer, model, createdTime);
        string RealFilePath = Path.Combine(FileDirPath, browserFile.Name);

        Photo Photo = ReadMetadata.GetPhoto(metaData, RealFilePath, browserFile.Name);
    
        string FileThumnailRoot = Path.Combine(GetThumbnailPath(), Photo.PhotoId.ToString());
        Photo.Thumbnail_path = FileThumnailRoot;
        GeneratePreview.CreateThumbnailFolder(FileThumnailRoot);

        using FileStream fs = new(RealFilePath, FileMode.Create);

        var bytesRead = 0;
        var totalRead = 0;
        var buffer = new byte[1024 * 10];

        while ((bytesRead = await ms.ReadAsync(buffer)) != 0)
        {
            totalRead += bytesRead;
            await fs.WriteAsync(buffer, 0, bytesRead);
        }

        return Photo;
    }

    public static Article GetArticle(Guid PhotoId)
    {
        return new Article {
            PhotoId = PhotoId
        };
    }

    public static Article GetArticle(Photo Photo)
    {
        return new Article {
            PhotoId = Photo.PhotoId
        };
    }
}