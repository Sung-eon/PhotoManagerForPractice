namespace PhotoManager.Api.v1.Service;

using PhotoManager.Data;
using System.Collections.Concurrent;
using PhotoManager.Api.v1.Helper.Images;

public interface IIndexingService
{
    public Task StartAsync(CancellationToken stoppingToken);
    public Task StopAsync(CancellationToken stoppingToken);
    public Task ClearAll();

    public int GetNumberOfCompletedWorks();
    public int GetNumberOfAllWorks();
    public decimal GetPercentageOfProgress();
    public WorkPhase GetWorkPhase();
    
}

public enum WorkPhase
{
    BeforeStart,
    Scaning,
    MetadataGeneration,
    MetadataInsertion,
    MetadataSaving,
    ThumbnailGeneration,
    PreviewGeneration,
    Finished
}

public class IndexingService : IIndexingService
{
    private string? PHOTO_ROOT = Environment.GetEnvironmentVariable("PHOTO_ROOT_DIR");
    private string? THUMBNAIL_ROOT = Path.Combine(Directory.GetCurrentDirectory(), "thumbnail");

    private EnumerationOptions SearchingOptions = new EnumerationOptions()
    {
        IgnoreInaccessible = true,
        RecurseSubdirectories = true,
        AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.SparseFile | FileAttributes.Temporary | FileAttributes.Archive | FileAttributes.Compressed
    };

    private ParallelOptions parallelOptions = new()
    {
        MaxDegreeOfParallelism = Math.Max(Convert.ToInt32(Math.Floor(Environment.ProcessorCount * 0.7)), 2)
    };

    private readonly ILogger<IndexingService> _iLogger;
    private readonly PhotoDbContext? _photoDbContext;
    private ConcurrentBag<Photo> cb;
    private ParallelLoopResult ParallelWork;
    private bool ParallelWorkState = false;
    private IEnumerable<string> FilePathes;
    
    private int CompletedWorks { get; set; }
    private int AllWorks { get; set; }
    private WorkPhase workPhase { get; set; } = WorkPhase.BeforeStart;

    public IndexingService(PhotoDbContext photoDbContext, ILogger<IndexingService> iLogger)
    {
        this._photoDbContext = photoDbContext;
        this._iLogger = iLogger;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        cb = new ConcurrentBag<Photo>();

        workPhase = WorkPhase.Scaning;

        IEnumerable<string> d = Directory.EnumerateFiles(PHOTO_ROOT, "*.*", SearchingOptions)
        .Where(s => !Path.GetFullPath(s).Contains("@eaDir"))
        .Where(s => !Path.GetFullPath(s).Contains("#recycle"))
        .Where(s => CheckExtensions.IsImage(s));

        AllWorks = d.Count();

        _iLogger.LogInformation("Generating metadata for indexing");
        workPhase = WorkPhase.MetadataGeneration;
        CompletedWorks = 0;

        foreach (var item in d)
        {
            cb.Add(ReadMetadata.Read(item));
            CompletedWorks++;
        }

        AllWorks = cb.Count();

        _iLogger.LogInformation("Fetching some data from database before doing indexing");

        FilePathes = this._photoDbContext.Photos.Select(s => s.PhotoId.ToString());

        _iLogger.LogInformation("Inserting metadata into the database");
        workPhase = WorkPhase.MetadataInsertion;
        CompletedWorks = 0;

        ParallelWorkState = true;

        foreach (Photo model in cb)
        {
            if (ParallelWorkState)
            {
                string FileThumnailRoot = Path.Combine(THUMBNAIL_ROOT, model.PhotoId.ToString());

                if (!Directory.Exists(FileThumnailRoot)) Directory.CreateDirectory(FileThumnailRoot);

                if (!FilePathes.Contains(model.File_path))
                {
                    model.Thumbnail_path = FileThumnailRoot;

                    // photo
                    this._photoDbContext.Photos.Add(model);

                    // article
                    this._photoDbContext.Articles.Add(new Article { PhotoId = model.PhotoId });
                }

                CompletedWorks++;
            }
        }

        _iLogger.LogInformation("Saving metadata into the database");
        workPhase = WorkPhase.MetadataSaving;
        CompletedWorks = 0;

        this._photoDbContext.SaveChanges();

        _iLogger.LogInformation("Generating thumbnails");
        workPhase = WorkPhase.ThumbnailGeneration;

        ParallelWork = Parallel.ForEach(cb, parallelOptions, async (f, state) =>
        {
            // Gracefully stopping the works
            if (!ParallelWorkState) state.Break();

            await GeneratePreview.ThumbnailWebP(f.File_path, f.Thumbnail_path);

            CompletedWorks++;
        });

        _iLogger.LogInformation("Generating preview files");
        workPhase = WorkPhase.PreviewGeneration;
        CompletedWorks = 0;

        ParallelWork = Parallel.ForEach(cb, parallelOptions, async (f, state) =>
        {
            // Gracefully stopping the works
            if (!ParallelWorkState) state.Break();

            await GeneratePreview.PreviewFiles(f.File_path, f.Thumbnail_path);

            CompletedWorks++;
        });

        _iLogger.LogInformation("All done");
        workPhase = WorkPhase.Finished;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        if (!ParallelWork.IsCompleted)
        {
            ParallelWorkState = false;
        }
        return Task.CompletedTask;
    }

    public Task ClearAll()
    {
        this._photoDbContext.Photos.RemoveRange(this._photoDbContext.Photos);
        this._photoDbContext.Articles.RemoveRange(this._photoDbContext.Articles);
        this._photoDbContext.ArticleTags.RemoveRange(this._photoDbContext.ArticleTags);

        this._photoDbContext.SaveChanges();

        Directory.Delete(THUMBNAIL_ROOT, true);
        Directory.CreateDirectory(THUMBNAIL_ROOT);

        return Task.CompletedTask;
    }

    public int GetNumberOfCompletedWorks()
    {
        return this.CompletedWorks;
    }

    public int GetNumberOfAllWorks()
    {
        return this.AllWorks;
    }

    public decimal GetPercentageOfProgress()
    {
        return decimal.Divide(this.CompletedWorks, this.AllWorks);
    }

    public WorkPhase GetWorkPhase()
    {
        return this.workPhase;
    }
}

public class BackgroundIndexing : BackgroundService
{
    private readonly ILogger<BackgroundIndexing> _logger;
    public IServiceProvider _services { get; }

    public BackgroundIndexing(IServiceProvider services, ILogger<BackgroundIndexing> logger)
    {
        this._services = services;
        this._logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using (var scope = _services.CreateScope())
        {
            _logger.LogInformation("Creating scope for consuming a background service : BackgroundIndexing");

            var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IndexingService>();

            await scopedProcessingService.StartAsync(cancellationToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}