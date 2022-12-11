namespace PhotoManager.Data;

using Microsoft.EntityFrameworkCore;

public class PhotoDbContext : DbContext
{
    public PhotoDbContext(DbContextOptions<PhotoDbContext> options) : base(options)
    {
    }

    public DbSet<Photo> Photos { get; set; }
    public DbSet<Album> Albums { get; set; }
    public DbSet<CameraModel> CameraBodys { get; set; }
    public DbSet<LensModel> Lens { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<ArticleTag> ArticleTags { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<History> Historys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<History>()
        .Property(s => s.Event)
        .HasConversion(
            value => value.ToString(), 
            value => (EventType)Enum.Parse(typeof(EventType), value) 
        );
    }
}
