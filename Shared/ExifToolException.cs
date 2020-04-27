

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
    }
}