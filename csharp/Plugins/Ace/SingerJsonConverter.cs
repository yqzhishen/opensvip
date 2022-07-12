using System;
using AceStdio.Resources;
using Newtonsoft.Json;

namespace AceStdio.Stream
{
    public class SingerJsonConverter : JsonConverter<string>
    {
        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
        {
            writer.WriteValue(AceSingers.GetId(value));
        }

        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var id = (long?) reader.Value ?? 0;
            return AceSingers.GetName((int)id);
        }
    }
}