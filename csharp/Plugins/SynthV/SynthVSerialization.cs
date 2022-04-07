using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SynthV.Model
{
    public class SVPointListJsonConverter : JsonConverter<List<Tuple<long, double>>>
    {
        public override void WriteJson(JsonWriter writer, List<Tuple<long, double>> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var (pos, val) in value)
            {
                writer.WriteValue(pos);
                writer.WriteValue(val);
            }
            writer.WriteEndArray();
        }

        public override List<Tuple<long, double>> ReadJson(
            JsonReader reader, Type objectType, List<Tuple<long, double>> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = serializer.Deserialize<object[]>(reader);
            if (obj == null)
            {
                return null;
            }
            var res = new List<Tuple<long, double>>();
            for (var i = 0; i < obj.Length; i += 2)
            {
                res.Add(new Tuple<long, double>((long) obj[i], (double) obj[i + 1]));
            }
            return res;
        }
    }

}
