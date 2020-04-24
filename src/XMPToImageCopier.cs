using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using XmpCore;

namespace PhotoTool
{
    class XMPToImageCopier
    {
        public static readonly string[] supportedExtensions = { ".gif", ".heic", ".jpg", ".jpeg", ".png" };

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

                    Process process = new Process();
                    process.StartInfo.FileName = @"exiftool.exe";
                    process.StartInfo.Arguments = string.Concat(" -overwrite_original \"-DateTimeOriginal=", xmpMetaCreateDate.ToIso8601String(), "\"", " \"", this.getCorrespondingImageFile(), "\"");
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();

                    string exifToolError = process.StandardError.ReadToEnd().Replace(Environment.NewLine, "");
                    if (!string.IsNullOrEmpty(exifToolError)) 
                        Program.MainLogger.Error($"exifToolError {exifToolError}");  
                    string exifToolOutput = process.StandardOutput.ReadToEnd().Replace(Environment.NewLine, "");                    
                    if (!string.IsNullOrEmpty(exifToolOutput)) 
                        Program.MainLogger.Verbose($"exifToolOutput {exifToolOutput}");                      

                    process.WaitForExit();   
                }
                else
                {
                    Program.MainLogger.Information($"Cant't find 'photoshop:DateCreated' property in namespace 'http://ns.adobe.com/photoshop/1.0/'");                
                }
            }         
        }

        private FileInfo getCorrespondingImageFile()
        {                   
            string imageFileExtension = supportedExtensions.Where(e => File.Exists(Path.ChangeExtension(XmpFile.FullName, e))).Single();
            return new FileInfo(Path.ChangeExtension(XmpFile.FullName, imageFileExtension));
        }        
    }
}