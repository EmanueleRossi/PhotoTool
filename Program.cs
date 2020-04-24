using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
                            if(FitsMask(file, args[1]))
                            {         
                                MainLogger.Information($"Processing...");                                                                                                
                                if (string.Equals(Path.GetExtension(file), ".xmp", StringComparison.OrdinalIgnoreCase))
                                {
                                    XMPToImageCopier copier = new XMPToImageCopier(new FileInfo(file));
                                    copier.copyDateCreatedToDateTimeOriginal();                                                                                                 
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
                MainLogger.Fatal("Error :(", ex);
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
