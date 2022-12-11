namespace PhotoManager.Api.v1.Controller;

using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using PhotoManager.Data;
using PhotoManager.Api.v1.Helper.Images;
using PhotoManager.Api.v1.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class Indexing : ControllerBase
{
    private string? PHOTO_ROOT = Environment.GetEnvironmentVariable("PHOTO_ROOT_DIR");
    private string? THUMBNAIL_ROOT = Path.Combine(Directory.GetCurrentDirectory(), "thumbnail");

    private EnumerationOptions SearchingOptions = new EnumerationOptions()
    {
        IgnoreInaccessible = true,
        RecurseSubdirectories = true,
        AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.SparseFile | FileAttributes.Temporary | FileAttributes.Compressed
    };

    private ParallelOptions parallelOptions = new()
    {
        MaxDegreeOfParallelism = Math.Max(Convert.ToInt32(Math.Floor(Environment.ProcessorCount * 0.7)), 2)
    };

    private int MetadataProgress = 0;
    private int ThumbnailProgress = 0;
    private int PreviewProgress = 0;

    private readonly IWebHostEnvironment _environment;
    private readonly PhotoDbContext? photoDbContext;
    private readonly ILogger<Indexing> _iLogger;

    private Task IndexingTask;
    CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
    CancellationToken CancelToken;

    public Indexing(
        IWebHostEnvironment environment,
        PhotoDbContext photoDbContext,
        ILogger<Indexing> iLogger
        )
    {
        this._environment = environment;
        this.photoDbContext = photoDbContext;
        this._iLogger = iLogger;

        //
        CancelToken = CancelTokenSource.Token;

    }

    [HttpPost("[action]")]
    public async Task<ActionResult> RegeneratePreview(Guid PhotoId)
    {
        Photo model = this.photoDbContext.Photos.Single(s => s.PhotoId == PhotoId);

        if (model is not null)
        {
            if (System.IO.File.Exists(model.File_path))
            {
                // Thumbnail
                await GeneratePreview.ThumbnailWebP(model.File_path, model.Thumbnail_path);

                // Preview files
                await GeneratePreview.PreviewFiles(model.File_path, model.Thumbnail_path);

                return StatusCode(201);
            }
            else
            {
                return NotFound();
            }
        }
        else
        {
            return NotFound();
        }
    }

    [HttpPost("[action]")]
    public async Task<ActionResult> Scan()
    {
        ConcurrentBag<Photo> cb = new();

        IEnumerable<string> d = Directory.EnumerateFiles(PHOTO_ROOT, "*.*", SearchingOptions)
        .Where(s => !Path.GetFullPath(s).Contains("@eaDir"))
        .Where(s => !Path.GetFullPath(s).Contains("#recycle"));

        /*
        if (this._Photos is not null)
        {
            // Filtering already indexed files to save time and resource.
            d = d.Where(s => !this._Photos.Any(t => t.File_path == s));
        }
        */

        _iLogger.LogInformation($"Find all files to index: {d.Count()}");
        //_indexingStatusMessenger.SendMessage($"Find all files to index: {d.Count()}");


        _iLogger.LogInformation("Generating metadata for indexing");

        foreach (var item in d)
        {
            if (CheckExtensions.IsImage(item))
            {
                Photo model = ReadMetadata.Read(item);
                cb.Add(model);
            }
        }

        _iLogger.LogInformation("Inserting metadata into the database");
        //_indexingStatusMessenger.SendMessage("Inserting metadata into the database");

        // Asynchronous
        foreach (Photo model in cb)
        {
            string FileThumnailRoot = Path.Combine(THUMBNAIL_ROOT, model.PhotoId.ToString());

            if (!Directory.Exists(FileThumnailRoot))
            {
                Directory.CreateDirectory(FileThumnailRoot);
            }

            if (!photoDbContext.Photos.Any(s => s.File_path == model.File_path))
            {
                model.Thumbnail_path = FileThumnailRoot;

                // photo
                await photoDbContext.Photos.AddAsync(model);

                // article
                Article article = new()
                {
                    PhotoId = model.PhotoId
                };
                await photoDbContext.Articles.AddAsync(article);

                MetadataProgress += 1;

                //_indexingStatusMessenger.SendMessage($"[Inserting metadata] {MetadataProgress} / {d.Count()} has been completed");
            }
        }

        await photoDbContext.SaveChangesAsync();

        // Assign a work
        IndexingTask = Task.Run(() =>
        {
            // Thumbnail
            Parallel.ForEach(cb, parallelOptions, async f =>
            {
                await GeneratePreview.ThumbnailWebP(f.File_path, f.Thumbnail_path);

                ThumbnailProgress += 1;

                //_indexingStatusMessenger.SendMessage($"[Generating thumbnails] {ThumbnailProgress} / {d.Count()} has been completed");           
            });

            // Preview
            Parallel.ForEach(cb, parallelOptions, async f =>
            {
                await GeneratePreview.PreviewFiles(f.File_path, f.Thumbnail_path);

                PreviewProgress += 1;

                //_indexingStatusMessenger.SendMessage($"[Generating previews] {PreviewProgress} / {d.Count()} has been completed");
            });
        });

        while (!IndexingTask.IsCompleted)
        {
            await Task.Delay(500);
        }

        if (IndexingTask.IsCompletedSuccessfully)
        {
            _iLogger.LogInformation("Finished all works");
            // _indexingStatusMessenger.SendMessage("Finished all works");

            return StatusCode(201);
        }
        else if (IndexingTask.IsCanceled)
        {
            _iLogger.LogInformation("Indexing work is cancelled");
            // _indexingStatusMessenger.SendMessage("Indexing work is cancelled");

            return BadRequest();
        }
        else if (IndexingTask.IsFaulted)
        {
            _iLogger.LogInformation("Indexing has failed");
            // _indexingStatusMessenger.SendMessage("Indexing has failed");

            return BadRequest();
        }
        else
        {
            _iLogger.LogInformation("Done");
            //_indexingStatusMessenger.SendMessage("Done");

            return StatusCode(200);
        }
    }

    [HttpPost("[action]")]
    public ActionResult Cancel()
    {
        if (IndexingTask is not null)
        {
            if (!IndexingTask.IsCompleted)
            {
                CancelTokenSource.Cancel();
            }
        }

        return StatusCode(200);
    }

    [HttpDelete("[action]")]
    public async Task<ActionResult> ClearAll()
    {
        try
        {
            photoDbContext.Photos.RemoveRange(photoDbContext.Photos);
            photoDbContext.Articles.RemoveRange(photoDbContext.Articles);
            photoDbContext.ArticleTags.RemoveRange(photoDbContext.ArticleTags);

            await photoDbContext.SaveChangesAsync();

            Directory.Delete(THUMBNAIL_ROOT, true);
            Directory.CreateDirectory(THUMBNAIL_ROOT);

            return StatusCode(201);
        }
        catch
        {
            return StatusCode(500);
        }
    }
}