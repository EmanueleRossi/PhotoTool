using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using PhotoTool.Shared;

namespace PhotoTool.Library
{
    public class LibraryWorker
    {
        public static readonly string[] supportedExtensions = { ".gif", ".heic", ".jpg", ".jpeg", ".png" };
        public static readonly string[] excludedExtensions = { ".html", ".json" };
        public List<LibraryImage> libraryImages { get; }

        public LibraryWorker(string[] paths)
        {
            libraryImages = new List<LibraryImage>();        
            foreach(string path in paths) 
            {
                if(File.Exists(path)) 
                {
                    this.ProcessFile(path); 
                }               
                else if(Directory.Exists(path)) 
                {
                    this.ProcessDirectory(path);
                }
                else 
                {
                    Program.MainLogger.Warning("{0} is not a valid file or directory.", path);
                }        
            }
        }

        private void ProcessDirectory(string targetDirectory) 
        {
            string [] fileEntries = Directory.GetFiles(targetDirectory);            
            foreach(string fileName in fileEntries)            
                this.ProcessFile(fileName);
            string [] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach(string subdirectory in subdirectoryEntries) 
                this.ProcessDirectory(subdirectory);            
        }

        private void ProcessFile(string imageFilePath)         
        {
            LibraryImage libraryImage = new LibraryImage();            
            string fileFullPath = Path.GetFullPath(imageFilePath);                 
            libraryImage.FileFullPath = fileFullPath;
            string extension = Path.GetExtension(imageFilePath).ToLower();        
            if (supportedExtensions.Contains(extension)) 
            {
                try
                {
                    Process process = new Process();
                    process.StartInfo.FileName = @"exiftool.exe";
                    process.StartInfo.Arguments = string.Concat(" -j -a -m ", "\"", fileFullPath, "\"");
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();

                    string exifToolError = process.StandardError.ReadToEnd().Replace(Environment.NewLine, "");
                    if (!string.IsNullOrEmpty(exifToolError)) throw new ExifToolException(exifToolError);

                    string exifToolOutput = process.StandardOutput.ReadToEnd();
                    if (!string.IsNullOrEmpty(exifToolOutput))                    
                    {
                        Program.MainLogger.Debug($"exifToolOutput {exifToolOutput}");
                        JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
                        serializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeParseAsFallback());
                        ExifToolResponse jsonExifToolOutput = JsonSerializer.Deserialize<ExifToolResponse[]>(exifToolOutput, serializerOptions).First();                
                        libraryImage.PhotoDate = jsonExifToolOutput.CreateDate;
                        libraryImage.Latitude = jsonExifToolOutput.GPSLatitude;
                        libraryImage.Longitude = jsonExifToolOutput.GPSLongitude;
                        libraryImage.Altitude = jsonExifToolOutput.GPSAltitude;                                                                                 
                    }                     
                                                    
                    process.WaitForExit();   

                    if (File.Exists(Path.ChangeExtension(fileFullPath, ".json")))
                    {
                        libraryImage.MetadataFileFullPath = fileFullPath + ".json";
                        var jsonMetadataString = File.ReadAllText(fileFullPath + ".json");
                        JsonDocument jsonMetadata = JsonDocument.Parse(jsonMetadataString);
                        
                        DateTime datetimeBegin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        double offset = Convert.ToDouble(jsonMetadata.RootElement.GetProperty("photoTakenTime").GetProperty("timestamp").GetString());
                        libraryImage.MetadataPhotoTakenTime = datetimeBegin.AddSeconds(offset).ToLocalTime();
                        
                        libraryImage.MetatdataLatitude = jsonMetadata.RootElement.GetProperty("geoData").GetProperty("latitude").GetDouble();
                        libraryImage.MetatdataLatitudeExif = jsonMetadata.RootElement.GetProperty("geoDataExif").GetProperty("latitude").GetDouble();
                        libraryImage.MetatdataLongitude = jsonMetadata.RootElement.GetProperty("geoData").GetProperty("longitude").GetDouble();
                        libraryImage.MetatdataLongitudeExif = jsonMetadata.RootElement.GetProperty("geoDataExif").GetProperty("longitude").GetDouble();                    
                    }    
                    libraryImages.Add(libraryImage);                
                } 
                catch (Exception e)
                {
                    Program.MainLogger.Error("Can't read EXIF data for file '{0}' : '{1}'.", fileFullPath, e.Message);
                }
            }       
            else if (!excludedExtensions.Contains(extension))  
            {
                Program.MainLogger.Error("Unmanaged extension: '{0}'.", extension);
            } 
        }
    }
}