using System;

namespace PhotoTool.Library
{
    public class LibraryImage
    {
        public string fileFullPath { get; set; }
        public string fileExtension { get; set; } 

        public DateTime? photoDate { get; set; }
        public string latitude { get; set; }    
        public string longitude { get; set; }
        public string altitude { get; set; }  
        public string metadataFileFullPath { get; set; }        
        public DateTime? metadataPhotoTakenTime { get; set; }
        public double? metatdataLatitude { get; set; }
        public double? metatdataLongitude { get; set; }
        public double? metatdataLatitudeExif { get; set; }
        public double? metatdataLongitudeExif { get; set; }             
    }
}