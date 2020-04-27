using System;

namespace PhotoTool.Shared
{
    public class ExifToolException : Exception
    {
        public ExifToolException()
        {
        }

        public ExifToolException(string message)
            : base(message)
        {
        }

        public ExifToolException(string message, Exception inner)
            : base(message, inner)
        {
        }        
    }
}