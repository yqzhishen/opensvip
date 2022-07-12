using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AceStdio.Stream
{
    public class ParamsJsonConverter : JsonConverter<List<double>>
    {
        public override void WriteJson(JsonWriter writer, List<double> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var point in value)
            {
                writer.WriteValue(point.ToString("F3"));
            }
            writer.WriteEndArray();
        }

        public override List<double> ReadJson(JsonReader reader, Type objectType, List<double> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jArr = serializer.Deserialize<JArray>(reader);
            return jArr?.Select(jToken => jToken.Value<double>()).ToList();
        }
    }
}