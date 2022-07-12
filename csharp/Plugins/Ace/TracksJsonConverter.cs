using System;
using System.Collections.Generic;
using AceStdio.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AceStdio.Stream
{
    public class TracksJsonConverter : JsonConverter<List<AceTrack>>
    {
        public override void WriteJson(JsonWriter writer, List<AceTrack> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override List<AceTrack> ReadJson(JsonReader reader, Type objectType, List<AceTrack> existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var jArr = serializer.Deserialize<JArray>(reader);
            if (jArr == null)
            {
                return null;
            }
            var trackList = new List<AceTrack>();
            foreach (var jToken in jArr)
            {
                var jObject = (JObject) jToken;
                switch (jObject.Value<string>("type"))
                {
                    case "sing":
                        trackList.Add(jObject.ToObject<AceVocalTrack>());
                        break;
                    case "audio":
                        trackList.Add(jObject.ToObject<AceAudioTrack>());
                        break;
                    // ignore empty tracks
                }
            }
            return trackList;
        }
    }
}
