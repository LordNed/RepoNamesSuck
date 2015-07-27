using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WEditor.WindWaker
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
            string [] rawKeywords = csvValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            List<string> strArray = new List<string>();
            for (int i = 0; i < rawKeywords.Length; i++)
            {
                if (rawKeywords[i].Trim().Length > 0)
                    strArray.Add(rawKeywords[i].Trim());
            }

            return strArray.ToArray();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanWrite is false.");
        }
    }
}
