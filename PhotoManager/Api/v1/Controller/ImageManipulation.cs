namespace PhotoManager.Api.v1.Controller;

using Microsoft.AspNetCore.Mvc;

using ImageMagick;
using PhotoManager.Data;

[ApiController]
[Route("api/v1/image/[controller]")]
public class Manipulation : ControllerBase, IDisposable
{
    private readonly PhotoDbContext _photoDbContext;
    private MemoryStream? ms;

    public Manipulation(PhotoDbContext photoDbContext)
    {
        this._photoDbContext = photoDbContext;
    }

    [HttpGet("[action]")]
    public async Task<ActionResult> GetLink(Guid PhotoId, int? Width, int? Height, string? Format)
    {
        Photo model = this._photoDbContext.Photos.Single(s => s.PhotoId == PhotoId);
        string FilePath = model.File_path;
        string FileExtension = Format;
        string FileName = Path.GetFileNameWithoutExtension(FilePath);

        string _format = Format ?? "jpg";

        int _width = Width ?? model.Width;
        int _height = Height ?? model.Height;

        ms = new MemoryStream();

        using MagickImage image = new(FilePath);

        if (model.Width != Width && model.Height != Height)
        {
            image.Resize(_width, _height);
        }

        MagickFormat format = _format switch
        {
            "png" => MagickFormat.Png,
            "webp" => MagickFormat.WebP,
            "heif" => MagickFormat.Heif,
            "bmp" => MagickFormat.Bmp,
            "tiff" => MagickFormat.Tiff,
            _ => MagickFormat.Jpg
        };

        await image.WriteAsync(ms, format);

        ms.Seek(0, SeekOrigin.Begin);

        return new FileStreamResult(ms, $"image/{_format}")
        {
            FileDownloadName = $"{FileName}.{_format}",
            LastModified = DateTimeOffset.Now
        };
    }

    public void Dispose()
    {
        ms = null;
    }
}