namespace PhotoManager.Data;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Index(nameof(File_path), IsUnique = true)]
[Index(nameof(Model), nameof(Maker))]
[Index(nameof(Export_datetime))]
public class Photo
{
    [Key]
    public Guid PhotoId { get; set; } = Guid.NewGuid();
    public int Height { get; set; }
    public int Width { get; set; }
    public int? Bit { get; set; }
    public string? Maker { get; set; }
    public string? Model { get; set; }
    public int? Dpi { get; set; }
    public string? Software { get; set; }
    public DateTime? Export_datetime { get; set; } = DateTime.Today;
    public int? Exposure { get; set; }
    public double? Aperture { get; set; }
    public string? Exposure_program { get; set; }
    public int? Iso { get; set; }
    public DateTime? Original_datetime { get; set; } = DateTime.Today;
    public string? Timezone_offset { get; set; }
    public double? Shutter_speed { get; set; }
    public string? Shutter_speed_text { get; set; }
    public string? Metering_mode { get; set; }
    public string? White_balance { get; set; }
    public double? Focal_length { get; set; }
    public string? Color_space { get; set; }
    public string? Exposure_mode { get; set; }
    public double? Focal_length_in_35 { get; set; }
    public string? Lens_model { get; set; }
    public string? Gps_latitude_ref { get; set; }
    public string? Gps_longitude_ref { get; set; }
    public double? Gps_latitude { get; set; }
    public double? Gps_longitude { get; set; }
    public string? Place_city { get; set; }
    public string? Place_sublocation { get; set; }
    public string? Place_province { get; set; }
    public string? Place_country { get; set; }
    public string? File_format { get; set; }
    public string? Mime { get; set; }
    public string File_name { get; set; }
    public string File_path { get; set; }
    public string Thumbnail_path { get; set; }

    public Article Article { get; set; }
}

[Index(nameof(Model_name), nameof(Maker), IsUnique = true)]
public class CameraModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Model_name { get; set; }
    public string Maker { get; set; }
    public string Sensor_type { get; set; }
}

[Index(nameof(Model_name), nameof(Maker), IsUnique = true)]
public class LensModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Model_name { get; set; }
    public string Maker { get; set; }
    public int Max_focal_length { get; set; }
    public int Min_focal_length { get; set; }
    public double Max_aperture { get; set; }
    public double Min_aperture { get; set; }
}