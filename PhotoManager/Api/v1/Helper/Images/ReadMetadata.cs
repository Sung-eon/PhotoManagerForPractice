namespace PhotoManager.Api.v1.Helper.Images;

using System.Text.RegularExpressions;
using System.Globalization;
using MetadataExtractor;
using PhotoManager.Data;
using Microsoft.AspNetCore.Components.Forms;

public static class ReadMetadata
{
    public static int ParseInt(string text)
    {
        return Int32.Parse(Regex.Match(text, @"\d+").Value);
    }

    public static double ParseDouble(string text)
    {
        return Double.Parse(Regex.Match(text, @"\d+(\.\d+)?").Value);
    }

    public static double ParseShutterSpeed(string? text)
    {
        string fractionPattern = @"\/\d+";
        string rationalPattern = @"\d+(\.\d+)?";

        if (text is not null)
        {
            if (Regex.IsMatch(text, fractionPattern))
            {
                decimal denominator = Decimal.Parse(Regex.Match(text, fractionPattern).Value.Replace("/", ""));

                return Decimal.ToDouble(Decimal.Divide(1, denominator));
            }
            else if (Regex.IsMatch(text, rationalPattern))
            {
                return Double.Parse(Regex.Match(text, rationalPattern).Value);
            }
            else
            {
                throw new Exception("Text to be parsed is not formatted properly");
            }
        }
        else
        {
            return 0;
        }
    }

    public static string? ParseShutterSpeedAsString(string text)
    {
        string fractionPattern = @"\d\/\d+\.?\d?";
        string rationalPattern = @"\d+(\.\d+)?";

        if (Regex.IsMatch(text, fractionPattern))
        {
            return Regex.Match(text, fractionPattern).Value;
        }
        else if (Regex.IsMatch(text, rationalPattern))
        {
            return Regex.Match(text, rationalPattern).Value;
        }
        else
        {
            return null;
        }
    }

    public static double ParseGPSCoordinate(string text)
    {
        IEnumerable<double> coord = text.Split(" ").Select(s => ParseDouble(s));
        return coord.ElementAt(0) + (coord.ElementAt(1) / 60) + (coord.ElementAt(2) / 3600);
    }

    public static (string, dynamic) TransformData((string, string) tag, string description)
    {
        CultureInfo provider = CultureInfo.InvariantCulture;

        string DateTimeFormat = "yyyy':'MM':'dd HH':'mm':'ss";

        // Console.WriteLine($"[ {tag.Item1} - {tag.Item2} ] {description}");

        switch (tag)
        {
            case (_, "Image Height"):
                return ("height", ParseInt(description));
            case (_, "Image Width"):
                return ("width", ParseInt(description));
            case (_, "Bits Per Sample"):
                return ("bit", ParseInt(description));
            case (_, "Data Precision"):
                return ("bit", ParseInt(description));
            case (_, "Make"):
                return ("maker", description);
            case (_, "Model"):
                return ("model", description);
            case ("Exif IFD0", "X Resolution"):
                return ("dpi", ParseInt(description));
            case (_, "Software"):
                return ("software", description);
            case (_, "Date/Time"):
                return ("export_datetime", DateTime.ParseExact(description, DateTimeFormat, provider));
            case (_, "F-Number"):
                return ("aperture", ParseDouble(description));
            case (_, "Exposure Program"):
                return ("exposure_program", description);
            case (_, "ISO Speed Ratings"):
                return ("iso", ParseInt(description));
            case (_, "Date/Time Original"):
                return ("original_datetime", DateTime.ParseExact(description, DateTimeFormat, provider));
            case (_, "Time Zone"):
                return ("timezone_offset", description);
            case ("Exif SubIFD", "Exposure Time"):
                return ("shutter_speed_text", ParseShutterSpeedAsString(description));
            case ("Exif SubIFD", "Shutter Speed Value"):
                return ("shutter_speed_text", ParseShutterSpeedAsString(description));
            case (_, "Metering Mode"):
                return ("metering_mode", description);
            case (_, "White Balance Mode"):
                return ("white_balance", description);
            case (_, "Focal Length"):
                return ("focal_length", ParseDouble(description));
            case ("Exif SubIFD", "Color Space"):
                return ("color_space", description);
            case (_, "Exposure Mode"):
                return ("exposure_mode", description);
            case (_, "Focal Length 35"):
                return ("focal_length_in_35", ParseDouble(description));
            case (_, "Lens Model"):
                return ("lens_model", description);
            case (_, "GPS Latitude Ref"):
                return ("gps_latitude_ref", description);
            case (_, "GPS Longitude Ref"):
                return ("gps_longitude_ref", description);
            case (_, "GPS Latitude"):
                return ("gps_latitude", ParseGPSCoordinate(description));
            case (_, "GPS Longitude"):
                return ("gps_longitude", ParseGPSCoordinate(description));
            case ("IPTC", "City"):
                return ("place_city", description);
            case ("IPTC", "Sub-location"):
                return ("place_sublocation", description);
            case ("IPTC", "Province/State"):
                return ("place_province", description);
            case ("IPTC", "Country/Primary Location Name"):
                return ("place_country", description);
            case ("File Type", "Detected File Type Name"):
                return ("file_format", description);
            case ("File Type", "Detected MIME Type"):
                return ("mime", description);
            case ("File", "File Name"):
                return ("file_name", description);
            default:
                return ("unknown", description);
        }
    }

    public static Dictionary<string, dynamic?> ParseData(IEnumerable<Directory> directories)
    {
        Dictionary<string, dynamic?> metaData = new();

        foreach (Directory directory in directories)
        {
            foreach (Tag tag in directory.Tags)
            {
                var data = TransformData((directory.Name, tag.Name), tag.Description);
                metaData[data.Item1] = data.Item2;
            }
        }

        metaData["shutter_speed"] = ParseShutterSpeed(metaData["shutter_speed_text"]);

        return metaData;
    }

    public static Dictionary<string, dynamic?> ExtractData(string path)
    {
        return ParseData(ImageMetadataReader.ReadMetadata(path));
    }

    public static Dictionary<string, dynamic?> ExtractData(Stream stream)
    {
        IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(stream);
        stream.Seek(0, SeekOrigin.Begin);

        return ParseData(directories);
    }

    public static async Task<Photo> Read(IBrowserFile browserFile)
    {
        long maxFileSize = 50 * 1024 * 1024;

        Stream BrowserFileStream = browserFile.OpenReadStream(maxFileSize);

        using (MemoryStream ms = new())
        {
            await BrowserFileStream.CopyToAsync(ms);

            ms.Seek(0, SeekOrigin.Begin);

            Dictionary<string, dynamic?> metaData = ExtractData(ms);

            return GetPhoto(metaData, "/", browserFile.Name);
        }
    }

    public static Photo GetPhoto(Dictionary<string, dynamic?> metaData, string path, string? file_name)
    {
        string unknown = "unknown";

        string FileName = file_name ?? metaData["file_name"];
        string FilePath = path;

        return new Photo()
        {
            Height = metaData["height"],
            Width = metaData["width"],
            Bit = metaData.GetValueOrDefault("bit", null),
            Maker = metaData.GetValueOrDefault("maker", unknown),
            Model = metaData.GetValueOrDefault("model", unknown),
            Dpi = metaData.GetValueOrDefault("dpi", null),
            Software = metaData.GetValueOrDefault("software", null),
            Export_datetime = metaData.GetValueOrDefault("export_datetime", null),
            Aperture = metaData.GetValueOrDefault("aperture", null),
            Exposure_program = metaData.GetValueOrDefault("exposure_program", null),
            Iso = metaData.GetValueOrDefault("iso", null),
            Original_datetime = metaData.GetValueOrDefault("original_datetime", null),
            Timezone_offset = metaData.GetValueOrDefault("timezone_offset", null),
            Shutter_speed_text = metaData.GetValueOrDefault("shutter_speed_text", null),
            Shutter_speed = metaData.GetValueOrDefault("shutter_speed", 0),
            Metering_mode = metaData.GetValueOrDefault("metering_mode", null),
            White_balance = metaData.GetValueOrDefault("white_balance", null),
            Focal_length = metaData.GetValueOrDefault("focal_length", null),
            Color_space = metaData.GetValueOrDefault("color_space", null),
            Exposure_mode = metaData.GetValueOrDefault("exposure_mode", null),
            Focal_length_in_35 = metaData.GetValueOrDefault("focal_length_in_35", null),
            Lens_model = metaData.GetValueOrDefault("lens_model", null),
            Gps_latitude_ref = metaData.GetValueOrDefault("gps_latitude_ref", null),
            Gps_longitude_ref = metaData.GetValueOrDefault("gps_longitude_ref", null),
            Gps_latitude = metaData.GetValueOrDefault("gps_latitude", null),
            Gps_longitude = metaData.GetValueOrDefault("gps_longitude", null),
            Place_city = metaData.GetValueOrDefault("place_city", null),
            Place_sublocation = metaData.GetValueOrDefault("place_sublocation", null),
            Place_province = metaData.GetValueOrDefault("place_province", null),
            Place_country = metaData.GetValueOrDefault("place_country", null),
            File_format = metaData.GetValueOrDefault("file_format", null),
            Mime = metaData.GetValueOrDefault("mime", null),
            File_name = FileName,
            File_path = FilePath,
        };
    }

    public static Photo Read(string path)
    {
        Dictionary<string, dynamic?> metaData = ExtractData(path);
        return GetPhoto(metaData, path, null);
    }
}