using Newtonsoft.Json;
using System;

namespace WindEditor.UI
{
    public class CSVStringToArray : JsonConverter
    {
        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
                return null;

            string csvValue = reader.Value as string;
            string[] array = csvValue.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
            return array;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanWrite is false.");
        }
    }
}
