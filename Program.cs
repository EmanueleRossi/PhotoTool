using Serilog;
using Serilog.Core;
using System;
using System.IO;
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
                    string[] files = null;                    
                    if (args?.Length == 1)
                        files = Directory.GetFiles(args[0], "*.*", SearchOption.TopDirectoryOnly);
                    if (args?.Length == 2)
                        files = Directory.GetFiles(args[0], args[1], SearchOption.TopDirectoryOnly);                                    

                    foreach (string file in files) 
                    {                        
                        MainLogger.Verbose(Environment.NewLine);                                                              
                        MainLogger.Information($"Processing file {file}...");                                                                                                
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
                }
                else
                {
                    MainLogger.Error($"At least one parameter (working directory), is needed!");                   
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
    }
}
