using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenSvip.Model;

namespace OpenSvip
{
    namespace Serialization
    {
        class TrackJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var obj = serializer.Deserialize<JObject>(reader);
                if (obj == null)
                {
                    return null;
                }
                Track track;
                var type = obj.Value<string>("Type");
                switch (type)
                {
                    case "Singing":
                        track = new SingingTrack
                        {
                            AISingerId = obj.Value<string>("AISingerId"),
                            ReverbPreset = obj.Value<string>("ReverbPreset"),
                            NoteList = obj.Value<List<Note>>("NoteList"),
                            EditedParams = obj.Value<Params>("EditedParams")
                        };
                        break;
                    case "Instrumental":
                        track = new InstrumentalTrack
                        {
                            AudioFilePath = obj.Value<string>("AudioFilePath"),
                            Offset = obj.Value<int>("Offset")
                        };
                        break;
                    default:
                        return null;
                }
                track.Title = obj.Value<string>("Title");
                track.Muted = obj.Value<bool>("Muted");
                track.Solo = obj.Value<bool>("Solo");
                track.Volume = obj.Value<double>("Volume");
                track.Pan = obj.Value<double>("Pan");
                return track;
            }

            public override bool CanConvert(Type objectType)
            {
                return true;
            }
        }
    }
}