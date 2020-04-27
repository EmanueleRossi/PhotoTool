using System;

namespace PhotoTool.Shared
{
    public class ExifToolResponse
    {
        public string SourceFile { get; set; }
        public DateTime? DateTimeOriginal { get; set; }
        public DateTime? CreateDate { get; set; }
        public string GPSLatitude { get; set; }
        public string GPSLongitude { get; set; }
        public string GPSAltitude { get; set; }
    }
}