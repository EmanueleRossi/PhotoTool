using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PhotoTool.Shared
{
    public class DateTimeConverterUsingDateTimeParseAsFallback : JsonConverter<DateTime>
    {
        private const string customFormat = "yyyy:MM:dd HH:mm:ss"; 
        
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (!reader.TryGetDateTime(out DateTime value))
            {
                Program.MainLogger.Verbose($"Trying to parse {reader.GetString()}"); 
                value = DateTime.ParseExact(reader.GetString(), customFormat, CultureInfo.InvariantCulture);
            }
            return value;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(customFormat));
        }
    }
}