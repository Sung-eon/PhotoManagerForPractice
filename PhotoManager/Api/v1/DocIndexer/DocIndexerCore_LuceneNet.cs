namespace PhotoManager.Api.v1.DocIndexer;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using LuceneDirectory = Lucene.Net.Store.Directory;

using PhotoManager.Data;
using Lucene.Net.Documents.Extensions;
using J2N;

public abstract class DocIndexConfig
{
    public LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;
    public Analyzer standardAnalyzer;

    public string basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    public string indexPath;
    public LuceneDirectory indexDir;

    public IndexWriterConfig indexWriteConfig;

    public DocIndexConfig(string _indexName)
    {
        this.standardAnalyzer = new StandardAnalyzer(luceneVersion);
        
        this.indexPath = Path.Combine(basePath, _indexName);
        this.indexDir = FSDirectory.Open(this.indexPath);

        this.indexWriteConfig = new IndexWriterConfig(this.luceneVersion, this.standardAnalyzer);
    }
}

public interface IPhotoIndexerCore
{
    bool IsExist();
    void IndexPhotoMetadata(Photo photo);
    void IndexPhotoMetadataAsync(Photo photo);
    void IndexMultiplePhotoMetadata(IEnumerable<Photo> photos);

    void Dispose();
}

public class PhotoIndexerCore : DocIndexConfig, IPhotoIndexerCore, IDisposable
{
    private IndexWriter _photoIndexWriter;

    public PhotoIndexerCore() : base("photo_indexer")
    {
        this._photoIndexWriter = new IndexWriter(base.indexDir, base.indexWriteConfig);
    }

    public bool IsExist()
    {
        try
        {
            return base.indexDir.FileLength("photo_indexer") >= 0;
        }
        catch
        {
            return false;
        }
    }

    public void IndexPhotoMetadata(Photo photo)
    {
        Document doc = new Document();

        doc.AddStringField("id", photo.PhotoId.ToString(), Field.Store.YES);
        doc.AddStringField("file_name", photo.File_name, Field.Store.YES);

        doc.AddInt32Field("height", photo.Height, Field.Store.YES);
        doc.AddInt32Field("width", photo.Width, Field.Store.YES);

        if (photo.Bit is not null) doc.AddInt32Field("bit", (int)photo.Bit, Field.Store.YES);
        if (photo.Shutter_speed is not null) doc.AddDoubleField("bit", (double)photo.Shutter_speed, Field.Store.YES);
        if (photo.Maker is not null) doc.AddStringField("maker", (string)photo.Maker, Field.Store.YES);
        if (photo.Model is not null) doc.AddStringField("model", (string)photo.Model, Field.Store.YES);
        if (photo.Dpi is not null) doc.AddInt32Field("dpi", (int)photo.Dpi, Field.Store.YES);
        if (photo.Software is not null) doc.AddStringField("software", (string)photo.Software, Field.Store.YES);
        if (photo.Export_datetime is not null) doc.AddInt64Field("export_datetime", photo.Export_datetime.Value.GetMillisecondsSinceUnixEpoch(), Field.Store.YES);
        if (photo.Timezone_offset is not null) doc.AddStringField("timezone_offset", (string)photo.Timezone_offset, Field.Store.YES);
        if (photo.Shutter_speed is not null) doc.AddDoubleField("shutter_speed", (double)photo.Shutter_speed, Field.Store.YES);
        if (photo.Metering_mode is not null) doc.AddStringField("metering_mode", (string)photo.Metering_mode, Field.Store.YES);
        if (photo.White_balance is not null) doc.AddStringField("white_balance", (string)photo.White_balance, Field.Store.YES);
        if (photo.Focal_length is not null) doc.AddDoubleField("focal_length", (double)photo.Focal_length, Field.Store.YES);
        if (photo.Color_space is not null) doc.AddStringField("color_space", (string)photo.Color_space, Field.Store.YES);
        if (photo.Exposure_mode is not null) doc.AddStringField("exposure_mode", (string)photo.Exposure_mode, Field.Store.YES);
        if (photo.Focal_length_in_35 is not null) doc.AddDoubleField("focal_length_in_35mm", (double)photo.Focal_length_in_35, Field.Store.YES);
        if (photo.Lens_model is not null) doc.AddStringField("lens_model", (string)photo.Lens_model, Field.Store.YES);
        if (photo.Gps_latitude is not null) doc.AddDoubleField("gps_latitude", (double)photo.Gps_latitude, Field.Store.YES);
        if (photo.Gps_longitude is not null) doc.AddDoubleField("gps_latitude", (double)photo.Gps_longitude, Field.Store.YES);

        if (photo.Place_city is not null) doc.AddStringField("place_city", (string)photo.Place_city, Field.Store.YES);
        if (photo.Place_sublocation is not null) doc.AddStringField("place_sublocation", (string)photo.Place_sublocation, Field.Store.YES);
        if (photo.Place_province is not null) doc.AddStringField("place_province", (string)photo.Place_province, Field.Store.YES);
        if (photo.Place_country is not null) doc.AddStringField("place_country", (string)photo.Place_country, Field.Store.YES);
                

        if (photo.Iso is not null) doc.AddInt32Field("iso", (int)photo.Iso, Field.Store.YES);

    }

    public async void IndexPhotoMetadataAsync(Photo photo)
    {
        Task.Run(() => this.IndexPhotoMetadata(photo));
    }

    public void IndexMultiplePhotoMetadata(IEnumerable<Photo> photos)
    {
        foreach (Photo photo in photos)
        {
            this.IndexPhotoMetadata(photo);
        }
    }

    public void Dispose()
    {
        // Dispose LuceneDirectory
        base.indexDir.Dispose();

        // Dispose IndexWriter
        this._photoIndexWriter.Dispose();
    }

}