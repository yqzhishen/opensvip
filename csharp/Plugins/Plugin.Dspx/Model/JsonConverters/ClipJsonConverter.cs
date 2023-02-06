using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Plugin.Dspx.Model
{
    public class ClipJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jArray = serializer.Deserialize<JArray>(reader);
            var clips = new List<DsClip>();
            foreach (var jToken in jArray)
            {
                if (jToken == null)
                    return null;
                var type = jToken.Value<string>("type");
                switch (type)
                {
                    case "audio":
                        clips.Add(jToken.ToObject<DsAudioClip>());
                        break;

                    case "singing":
                        clips.Add(jToken.ToObject<DsSingingClip>());
                        break;

                    default:
                        break;
                }
            }
            return clips;
            //var jobj = serializer.Deserialize<JObject>(reader);
            //if (jobj == null)
            //    return null;
            //DsClip clip;
            //switch (type)
            //{
            //    case "audio":
            //        clip = new DsAudioClip
            //        {
            //            AudioFilePath= jobj.Value<string>("path")
            //        };
            //        break;

            //    case "singing":
            //        clip = new DsSingingClip
            //        {
            //            Notes = jobj.Value<JArray>("notes")?.ToObject<List<DsNote>>(),
            //            Params = jobj.Value<JObject>("params")?.ToObject<DsParams>(),
            //            Sources = jobj.Value<JObject>("sources")?.ToObject<DsSources>(),
            //        };
            //        break;

            //    default:
            //        return null;
            //}
            //clip.Time = jobj.Value<JObject>("time")?.ToObject<DsTime>();
            //clip.Name = jobj.Value<string>("name");
            //clip.Control = jobj.Value<JObject>("control")?.ToObject<DsControl>();
            //clip.Extra = jobj.Value<JObject>("extra")?.ToObject<DsExtra>();
            //clip.Workspace = jobj.Value<JObject>("workspace")?.ToObject<DsWorkspace>();
            //return clip;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}