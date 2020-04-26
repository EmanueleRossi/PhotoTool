using System;

namespace PhotoTool.Copier
{
    public class ExifToolResponse
    {
        public string SourceFile { get; set; }
        public DateTime? DateTimeOriginal { get; set; }
    }
}