namespace PhotoManager.Api.v1.Helper.Images;

using System;
using PhotoManager.Data;

public class DeletePhoto : IDisposable
{
    private readonly PhotoDbContext? _photoDbContext;
    private IQueryable<Photo>? photos;
    private IQueryable<Article>? articles;
    private IQueryable<Album>? albums;

    public DeletePhoto(PhotoDbContext photoDbContext)
    {
        this._photoDbContext = photoDbContext;

        this.photos = this._photoDbContext.Photos;
        this.articles = this._photoDbContext.Articles;
        this.albums = this._photoDbContext.Albums;
    }

    public async Task<int> DeleteFromDB(Guid PhotoID)
    {
        var model = this.photos.Single(s => s.PhotoId == PhotoID);
        return await this.DeleteFromDB(model);
    }

    public async Task<int> DeleteFromDB(Photo Photo)
    {
        this._photoDbContext.Remove(Photo);
        return await this._photoDbContext.SaveChangesAsync();
    }

    private void DeleteFolder(string Folder)
    {
        if (Directory.Exists(Folder))
        {
            Directory.Delete(Folder, true);
        }
    }

    private void DeleteFile(string FilePath)
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }

    public void DeleteRelateFolders(Guid PhotoId)
    {
        var model = this.photos.Single(s => s.PhotoId == PhotoId);
        this.DeleteRelateFolders(model);
    }

    public void DeleteRelateFolders(Photo Photo)
    {
        this.DeleteFolder(Photo.Thumbnail_path);
        this.DeleteFile(Photo.File_path);
    }

    public async Task<bool> Delete(Guid PhotoId)
    {
        try 
        {
            // Delete folders before deleting rows in the database server.
            DeleteRelateFolders(PhotoId);

            // Delete rows from the DB.
            await DeleteFromDB(PhotoId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> Delete(Photo Photo)
    {
        try 
        {
            // Delete folders before deleting rows in the database server.
            DeleteRelateFolders(Photo);

            // Delete rows from the DB.
            await DeleteFromDB(Photo);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        photos = null;
        albums = null;
        articles = null;
    }
}