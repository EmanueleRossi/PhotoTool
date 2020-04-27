using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PhotoTool.Shared
{
    public class ExifToolWrapper
    {
        private const string exifToolCmd = @"exiftool.exe"; 

        public ExifToolResponse execute(string exifToolArgument, FileInfo file)
        {
            ExifToolResponse jsonExifToolOutput = null;

            Process process = new Process();
            process.StartInfo.FileName = exifToolCmd;
            process.StartInfo.Arguments = string.Concat(" ", exifToolArgument, " \"", file.FullName, "\"");
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            string exifToolOutput = process.StandardOutput.ReadToEnd().Replace(Environment.NewLine, "");               
            string exifToolError = process.StandardError.ReadToEnd().Replace(Environment.NewLine, "");   
            process.WaitForExit(); 

            if (process.ExitCode == 0) 
            {                 
                if (!string.IsNullOrEmpty(exifToolOutput)) 
                {
                    if (exifToolOutput.EndsWith("image files updated"))
                    {
                        Program.MainLogger.Information($"exifToolOutput=|{exifToolOutput}|");
                    }
                    else
                    {
                        JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
                        serializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeParseAsFallback());
                        jsonExifToolOutput = JsonSerializer.Deserialize<ExifToolResponse[]>(exifToolOutput, serializerOptions).First();  
                    }              
                }
                else
                {
                    throw new ExifToolException($"Process Exit Code {process.ExitCode} but exifToolOutput empty!");    
                }
            }
            else
            {              
                Program.MainLogger.Error($"Error reading |{file}|, Exit Code=|{process.ExitCode}|, exifToolError=|{exifToolError}|");
                throw new ExifToolException($"Process Exit Code {process.ExitCode} but output empty!"); 
            }    

            return jsonExifToolOutput;
        }        
    }
}