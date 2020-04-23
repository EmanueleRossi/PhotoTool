using System;
using System.IO;

namespace PhotoTool
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(".");
            foreach (string file in files) 
            {
                Console.WriteLine($"Processing file {file}");
            }            
        }
    }
}
