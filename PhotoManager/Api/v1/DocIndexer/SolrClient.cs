namespace PhotoManager.Api.v1.DocIndexer;

using System.Net.Http;
using PhotoManager.Data;
using System.Text.Json;

public class SolrClient
{
    private static readonly HttpClient httpClient = new HttpClient();

    private readonly string solrID;
    private readonly string solrPW;

    public SolrClient(string id, string pw)
    {
        solrID = id;
        solrPW = pw;
    }

    private string Serialize(Photo photoModel) => JsonSerializer.Serialize<Photo>(photoModel);

}