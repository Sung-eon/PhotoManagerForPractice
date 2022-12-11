namespace PhotoManager.Pages.Directory.ByTag;

public class SearchBoxType
{
    private List<string> Tags = new();
    public string text 
    { 
        get => String.Join(" ", Tags);
        set 
        {
            this.Tags = value.Split(" ").Distinct().ToList();
        }
    }

    public void Append(string tag)
    {
        if (!this.Tags.Contains(tag))
        {
            this.Tags.Add(tag);
        }
    }

    public string GetParamUrl() => String.Join("&", this.Tags.Select(s => $"name={s}"));
    public string GetTagSearchUrl() => $"/directory/tag/find?{this.GetParamUrl()}";
}