using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

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
            libraryImage.fileFullPath = fileFullPath;
            string extension = Path.GetExtension(imageFilePath).ToLower();        
            libraryImage.fileExtension = extension;            
            if (supportedExtensions.Contains(extension)) 
            {
                try
                {
                    Process process = new Process();
                    process.StartInfo.FileName = @"exiftool.exe";
                    process.StartInfo.Arguments = string.Concat(" -j -a ", "\"", fileFullPath, "\"");
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();
                    string jOutput = process.StandardOutput.ReadToEnd();
                    if (!string.IsNullOrEmpty(jOutput))                    
                    {
                        JsonDocument jsonOutput = JsonDocument.Parse(jOutput);

                        bool hasCreateDateJsonElement =  jsonOutput.RootElement.EnumerateArray().ElementAtOrDefault(0).TryGetProperty("CreateDate", out JsonElement CreateDateJsonElement);
                        if (hasCreateDateJsonElement) 
                            libraryImage.photoDate = DateTime.ParseExact(CreateDateJsonElement.GetString(), "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);

                        bool hasGPSLatitudeJsonElement = jsonOutput.RootElement.EnumerateArray().ElementAtOrDefault(0).TryGetProperty("GPSLatitude", out JsonElement GPSLatitudeJsonElement);
                        if (hasGPSLatitudeJsonElement)
                            libraryImage.latitude = GPSLatitudeJsonElement.GetString();

                        bool hasGPSLongitudeJsonElement = jsonOutput.RootElement.EnumerateArray().ElementAtOrDefault(0).TryGetProperty("GPSLatitude", out JsonElement GPSLongitudeJsonElement);
                        if (hasGPSLongitudeJsonElement)
                            libraryImage.longitude = GPSLongitudeJsonElement.GetString();

                        bool hasGPSAltitudeJsonElement = jsonOutput.RootElement.EnumerateArray().ElementAtOrDefault(0).TryGetProperty("GPSAltitude", out JsonElement GPSAltitudeJsonElement);
                        if (hasGPSAltitudeJsonElement)
                            libraryImage.altitude = GPSAltitudeJsonElement.GetString();                                                                                   
                    }
                    string err = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(err)) 
                        Console.WriteLine(err);                      
                    process.WaitForExit();   

                    if (File.Exists(Path.ChangeExtension(fileFullPath, ".json"))
                    {
                        libraryImage.metadataFileFullPath = fileFullPath + ".json";
                        var jsonMetadataString = File.ReadAllText(fileFullPath + ".json");
                        JsonDocument jsonMetadata = JsonDocument.Parse(jsonMetadataString);
                        
                        DateTime datetimeBegin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        double offset = Convert.ToDouble(jsonMetadata.RootElement.GetProperty("photoTakenTime").GetProperty("timestamp").GetString());
                        libraryImage.metadataPhotoTakenTime = datetimeBegin.AddSeconds(offset).ToLocalTime();
                        
                        libraryImage.metatdataLatitude = jsonMetadata.RootElement.GetProperty("geoData").GetProperty("latitude").GetDouble();
                        libraryImage.metatdataLatitudeExif = jsonMetadata.RootElement.GetProperty("geoDataExif").GetProperty("latitude").GetDouble();
                        libraryImage.metatdataLongitude = jsonMetadata.RootElement.GetProperty("geoData").GetProperty("longitude").GetDouble();
                        libraryImage.metatdataLongitudeExif = jsonMetadata.RootElement.GetProperty("geoDataExif").GetProperty("longitude").GetDouble();                    
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