namespace PhotoManager.Pages.Admin;

using PhotoManager.Data;
using PhotoManager.Api.v1.Helper.Images;
using PhotoManager.Api.v1.Messages;
using Microsoft.EntityFrameworkCore;

public partial class Assets_Photo_Detail
{
    private string ReadonlyStyle = "basis-4/5 w-full py-2 px-3 text-gray-500";
    private string TextBoxStyle = "basis-4/5 w-full py-2 px-3 text-gray-500 border rounded";
    private bool HiddenSwitch = true;

    private class NewTag
    {
        public string Text { get; set; } = String.Empty;
    }

    private async void AddArticleTag()
    {
        var newTag = new ArticleTag { 
            Name = newTagString.Text,
            ArticleId = article.ArticleId
        };

        if (!photoDbContext.ArticleTags.Any(s => s.Name == newTagString.Text && s.ArticleId == article.ArticleId))
        {
            await photoDbContext.AddAsync(newTag);
            await photoDbContext.SaveChangesAsync();
        }

        newTagString.Text = String.Empty;
        StateHasChanged();
    }

    private async void RemoveArticleTag(ArticleTag tag)
    {
        photoDbContext.Remove(tag);
        await photoDbContext.SaveChangesAsync();

        StateHasChanged();
    }

    private string ChangeFocalLength
    {
        get => String.Format("{0:F1}", photo.Focal_length);
        set
        {
            double old_focal_length = (double)photo.Focal_length;

            photo.Focal_length = Double.Parse(value);

            double ratio = (double)photo.Focal_length_in_35 / old_focal_length;
            double newFocalIn35 = ratio * (double)photo.Focal_length;

            photo.Focal_length_in_35 = Math.Round(newFocalIn35, 1);
        }
    }

    private async void RegisterMetadata()
    {
        if (photoDbContext is not null)
        {
            photo.Shutter_speed = ReadMetadata.ParseShutterSpeed(photo.Shutter_speed_text);

            photoDbContext.Photos.Update(photo);
            await photoDbContext.SaveChangesAsync();
        }
    }

    private async void ResetMetadata()
    {
        if (photoDbContext is not null)
        {
            Photo newPhoto = ReadMetadata.Read(photo.File_path);
            newPhoto.PhotoId = photo.PhotoId;

            photoDbContext.Entry(photo).CurrentValues.SetValues(newPhoto);

            photoDbContext.Update(photo);
            await photoDbContext.SaveChangesAsync();
        }
    }

    private async void EditArticle()
    {

        Popup.SetMessage("Editing article... please wait until done", Severity.low);
        StateHasChanged();

        if (article.Publish_time.Ticks == 0)
        {
            article.Publish_time = DateTime.Now;
        }

        article.Edit_time = DateTime.Now;

        photoDbContext.Articles.Update(article);
        await photoDbContext.SaveChangesAsync();

        Popup.SetMessage("Successfully done!", Severity.normal);
        StateHasChanged();
    }

    private async void DeletePhoto()
    {
        var deletePhoto = new DeletePhoto(photoDbContext);

        await deletePhoto.Delete(photo);

        Navigator.NavigateTo("/admin/assets/photos");
    }

    public void ChangeModalHiddenState()
    {
        HiddenSwitch = !HiddenSwitch;
    }

}