using System;

namespace PhotoTool.Library
{
    public class LibraryImage
    {
        public string FileFullPath { get; set; }

        public DateTime? PhotoDate { get; set; }
        public string Latitude { get; set; }    
        public string Longitude { get; set; }
        public string Altitude { get; set; }  
        public string MetadataFileFullPath { get; set; }        
        public DateTime? MetadataPhotoTakenTime { get; set; }
        public double? MetatdataLatitude { get; set; }
        public double? MetatdataLongitude { get; set; }
        public double? MetatdataLatitudeExif { get; set; }
        public double? MetatdataLongitudeExif { get; set; }             
    }
}