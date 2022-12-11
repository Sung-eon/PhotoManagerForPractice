namespace PhotoManager.Data;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Index(nameof(Name), IsUnique = true)]
public class Album
{
    [Key]
    public Guid AlbumId { get; set; } = Guid.NewGuid();
    public string Name { get; set; }

    public ICollection<Article> Articles { get; set; }
}

[Index(nameof(PhotoId), IsUnique = true)]
public class Article
{
    [Key]
    public Guid ArticleId { get; set; } = Guid.NewGuid();
    public string? Content { get; set; }
    public DateTime Publish_time { get; set; } = DateTime.Now;
    public DateTime Edit_time { get; set; } = DateTime.Now;

    public Guid PhotoId { get; set; }
    public Photo Photo { get; set; }
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Album> Albums { get; set; } = new List<Album>();
    public ICollection<History> Historys { get; set; } = new List<History>();
}

public class Comment
{
    [Key]
    public Guid CommentId { get; set; } = Guid.NewGuid();
    public string Author { get; set; }
    public string? UserId { get; set; }
    public string Content { get; set; }
    public DateTime Publish_time { get; set; } = DateTime.Now;
    public DateTime Edit_time { get; set; } = DateTime.Now;
    public string? Password { get; set; }

    public Guid ArticleId { get; set; }
    public Article Article { get; set; }
}

public class ArticleTag
{
    public Guid ArticleTagId { get; set; } = Guid.NewGuid();
    public string Name { get; set; }

    public Guid ArticleId { get; set; }
    public Article Article { get; set; }
}

public enum EventType
{
    View,
    Like
}

public class History
{
    [Key]
    public Guid HistoryId { get; set; } = Guid.NewGuid();
    public DateTime? EventDateTime { get; set; } = DateTime.Now;
    public string? UserId { get; set; }
    public EventType Event { get; set; }

    public Guid ArticleId { get; set; }
    public Article Article { get; set; }
}