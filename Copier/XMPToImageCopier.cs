using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PhotoTool.Shared;
using XmpCore;

namespace PhotoTool.Copier
{
    class XMPToImageCopier
    {
        public static readonly string[] supportedExtensions = { ".jpg", ".jpeg", ".mp4", ".mov", ".heic" };

        private FileInfo XmpFile;

        public XMPToImageCopier(FileInfo xmpFile)
        {            
            XmpFile = xmpFile;
        }

        public void copyDateCreatedToDateTimeOriginal()
        {
            IXmpMeta xmpMeta;
            using (var fileStream = File.OpenRead(XmpFile.FullName))
            {
                xmpMeta = XmpMetaFactory.Parse(fileStream);
                if (xmpMeta.DoesPropertyExist("http://ns.adobe.com/photoshop/1.0/", "photoshop:DateCreated"))
                {
                    IXmpDateTime xmpMetaCreateDate = xmpMeta.GetPropertyDate("http://ns.adobe.com/photoshop/1.0/", "photoshop:DateCreated");
                    List<FileInfo> correspondingImageFiles = this.getCorrespondingImageFile();
                    foreach(FileInfo correspondingImageFile in correspondingImageFiles)
                    {
                        DateTime? DateTimeOriginal = this.getDateTimeOriginal(correspondingImageFile);
                        if (DateTimeOriginal == null)
                        {
                            ExifToolWrapper exifTool = new ExifToolWrapper();
                            ExifToolResponse jsonExifToolOutput = exifTool.execute(string.Concat("-m -S -overwrite_original \"-DateTimeOriginal=", xmpMetaCreateDate.ToIso8601String(), "\""), correspondingImageFile);
                        }
                        else
                        {
                            Program.MainLogger.Information($"DateTimeOriginal TAG in file {correspondingImageFile} was already set to {DateTimeOriginal}");                      
                        }
                    }
                }
                else
                {
                    Program.MainLogger.Information($"Cant't find 'photoshop:DateCreated' property in namespace 'http://ns.adobe.com/photoshop/1.0/'");                
                }
            }         
        }

        private DateTime? getDateTimeOriginal(FileInfo imageFile)
        {
            ExifToolWrapper exifTool = new ExifToolWrapper();
            ExifToolResponse jsonExifToolOutput = exifTool.execute(" -j -m -q -DateTimeOriginal", imageFile);
            return jsonExifToolOutput.DateTimeOriginal;        
        }        

        private List<FileInfo> getCorrespondingImageFile()
        {                   
            List<string> imageFileExtensions = supportedExtensions.Where(e => File.Exists(Path.ChangeExtension(XmpFile.FullName, e))).ToList();
            return imageFileExtensions.Select(e => new FileInfo(Path.ChangeExtension(XmpFile.FullName, e))).ToList();
        }        
    }
}