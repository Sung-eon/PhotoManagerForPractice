namespace PhotoManager.Pages.View;

using PhotoManager.Data;
using PhotoManager.Api.v1.Helper.Auth;
using System.Collections.Generic;

public partial class SingleImage
{
    string ArticlePropertyTitleStyle = "mb-3 text-lg font-semibold";

    private class AlbumSearchBoxInput
    {
        public string albumName { get; set; }
    }
    AlbumSearchBoxInput albumSearchBoxInput = new();
    string? AlbumSearchOrCreateAlertMessage;

    private async void CreateNewAlbum()
    {
        Album newAlbum = new()
        {
            Name = albumSearchBoxInput.albumName
        };

        try
        {
            article.Albums.Add(newAlbum);
        }
        catch (NullReferenceException)
        {
            article.Albums = new List<Album> { newAlbum };
        }

        photoDbContext.Entry<Article>(article).CurrentValues.SetValues(article);

        //photoDbContext.Update(article);

        await photoDbContext.SaveChangesAsync();
    }

    private void ResetComment()
    {
        newComment = new Comment
        {
            ArticleId = article.ArticleId,
            Author = username ?? String.Empty,
            UserId = userId ?? String.Empty
        };
    }

    private bool CheckComment()
    {
        if (newComment.Author is null)
        {
            CommentRegisterErrorMessage = "Set your nickname!";
            return false;
        }
        else if (newComment.Password is null)
        {
            CommentRegisterErrorMessage = "Enter your password!";
            return false;
        }
        else if (newComment.Content is null)
        {
            CommentRegisterErrorMessage = "Write comment body!";
            return false;
        }

        CommentRegisterErrorMessage = null;
        return true;
    }

    private async void RegisterComment()
    {
        if (username is null)
        {
            if (CheckComment())
            {
                newComment.Password = PasswordHash.Encode(
                    text: newComment.Password,
                    key: newComment.Publish_time.ToLongDateString()
                );

                photoDbContext.Comments.Add(newComment);
            }
        }
        else
        {
            photoDbContext.Comments.Add(newComment);
        }

        await photoDbContext.SaveChangesAsync();

        this.ResetComment();

        StateHasChanged();
    }

    public void ChangeModalHiddenState()
    {
        HiddenSwitch = !HiddenSwitch;
    }

    private void OpenCommentModalHiddenState(Comment comment)
    {
        this.EditInProgressComment = comment;
        this.HiddenCommentModalSwitch = false;
    }

    private void CloseCommentModalHiddenState()
    {
        this.EditInProgressComment = null;
        this.HiddenCommentModalSwitch = true;
    }

    private bool IdenticalDateTime()
    {
        long origin_datetime = photo.Original_datetime.Value.Ticks;
        long export_datetime = photo.Export_datetime.Value.Ticks;

        return origin_datetime == export_datetime;
    }




}