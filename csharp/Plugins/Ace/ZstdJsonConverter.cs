using System;
using System.Text;
using AceStdio.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZstdSharp;

namespace AceStdio.Stream
{
    public class ZstdJsonConverter : JsonConverter<AceContent>
    {
        public override void WriteJson(JsonWriter writer, AceContent value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override AceContent ReadJson(JsonReader reader, Type objectType, AceContent existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var contentString = reader.Value?.ToString();
            if (contentString == null)
                return null;
            
            var rawContent = Convert.FromBase64String(contentString);
            Span<byte> decompressed;
            using (var decompressor = new Decompressor())
                decompressed = decompressor.Unwrap(rawContent);
            var rawString = Encoding.UTF8.GetString(decompressed.ToArray());
            var content = JObject.Parse(rawString).ToObject<AceContent>();
            return content;
        }
    }
}