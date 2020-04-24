using Serilog;
using Serilog.Core;
using System;
using System.IO;

namespace PhotoTool
{
    class Program
    {
        public static Logger MainLogger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        static void Main(string[] args)
        {            
            if (args.Length != 0)
            {
                if (!string.IsNullOrWhiteSpace(args[0]))
                {
                    string[] files = Directory.GetFiles(args[0]);
                    foreach (string file in files) 
                    {
                        try
                        {
                            Log.Information($"Processing file {file}");                
                            if (string.Equals(Path.GetExtension(file), ".xmp", StringComparison.OrdinalIgnoreCase))
                            {
                                XMPToImageCopier copier = new XMPToImageCopier(new FileInfo(file));
                                copier.copyDateCreatedToDateTimeOriginal();                                                                                                 
                            }
                            else
                            {
                                Log.Information($"Not an XMP file!");                
                            }    
                        } 
                        catch (Exception ex)
                        {
                            MainLogger.Error($"Exception :(", ex);     
                        }        
                    }  
                }
                else
                {
                    MainLogger.Error($"First parameter cannot be empty!");   
                }
            }
            else
            {
                MainLogger.Error($"At least one parameter is needed!");                   
            }          
        }
    }
}
