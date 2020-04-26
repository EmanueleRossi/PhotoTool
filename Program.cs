using Serilog;
using Serilog.Core;
using System;
using System.IO;
using System.Text.RegularExpressions;
using PhotoTool.Copier;

namespace PhotoTool
{
    class Program
    {
        public static Logger MainLogger = new LoggerConfiguration()
            .MinimumLevel.Information()            
            .WriteTo.Console()
            .CreateLogger();

        static void Main(string[] args)
        {           
            try
            { 
                if (args?.Length > 0)
                {                    
                    string[] files = Directory.GetFiles(args[0]);
                    foreach (string file in files) 
                    {                        
                        MainLogger.Verbose(Environment.NewLine);                                                              
                        MainLogger.Verbose($"Analyzing file {file}");                                                              
                        if (args?.Length > 1)
                        {
                            // FIXME
                            if(file.Contains(args[1]))
                            {         
                                MainLogger.Information($"Processing...");                                                                                                
                                if (string.Equals(Path.GetExtension(file), ".xmp", StringComparison.OrdinalIgnoreCase))
                                {
                                    FileInfo xmpFileInfo = new FileInfo(file);
                                    XMPToImageCopier copier = new XMPToImageCopier(xmpFileInfo);
                                    copier.copyDateCreatedToDateTimeOriginal();      
                                    File.Move(file, Path.ChangeExtension(xmpFileInfo.FullName, ".xmp.done"), true);
                                }
                                else
                                {
                                    MainLogger.Information($"Not an XMP file!");                
                                }     
                            } 
                            else
                            {
                                MainLogger.Verbose($"File {file} does not matches pattern {args[1]}.");   
                            }                                                         
                        }
                        else
                        {
                            MainLogger.Error($"Parameter #2 is needed!");                   
                        }
                    }  
                }
                else
                {
                    MainLogger.Error($"Parameter #1 is needed!");                   
                }          
            } 
            catch (Exception ex) 
            {
                MainLogger.Fatal($"Error :( {ex.Message} {ex.StackTrace}");
            }
            finally
            {
                MainLogger.Dispose();
            }            
        }

        private static bool FitsMask(string fileName, string fileMask)
        {
            Regex mask = new Regex('^' + fileMask.Replace(".", "[.]").Replace("*", ".*").Replace("?", ".") + '$', RegexOptions.IgnoreCase);
            return mask.IsMatch(fileName);
        }   
    }
}
