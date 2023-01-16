using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenSvip.Model;

namespace OpenSvip.Serialization
{
    public class TrackJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var track = (Track)value;
            var dict = new Dictionary<string, object>
            {
                {"Type", track.Type},
                {"Title", track.Title},
                {"Mute", track.Mute},
                {"Solo", track.Solo},
                {"Volume", track.Volume},
                {"Pan", track.Pan}
            };
            switch (track)
            {
                case SingingTrack singingTrack:
                    dict["AISingerName"] = singingTrack.AISingerName;
                    dict["ReverbPreset"] = singingTrack.ReverbPreset;
                    dict["NoteList"] = singingTrack.NoteList;
                    dict["EditedParams"] = singingTrack.EditedParams;
                    break;
                case InstrumentalTrack instrumentalTrack:
                    dict["AudioFilePath"] = instrumentalTrack.AudioFilePath;
                    dict["Offset"] = instrumentalTrack.Offset;
                    break;
            }
            serializer.Serialize(writer, dict);
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
                        AISingerName = obj.Value<string>("AISingerName"),
                        ReverbPreset = obj.Value<string>("ReverbPreset"),
                        NoteList = obj.Value<JArray>("NoteList")?.ToObject<List<Note>>(),
                        EditedParams = obj.Value<JObject>("EditedParams")?.ToObject<Params>()
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
            track.Mute = obj.Value<bool>("Mute");
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

    public class PointListJsonConverter : JsonConverter<List<Tuple<int, int>>>
    {
        public override void WriteJson(JsonWriter writer, List<Tuple<int, int>> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var (pos, val) in value)
            {
                writer.WriteStartArray();
                writer.WriteValue(pos);
                writer.WriteValue(val);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }

        public override List<Tuple<int, int>> ReadJson(
            JsonReader reader, Type objectType, List<Tuple<int, int>> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = serializer.Deserialize<int[][]>(reader);
            return obj?.Select(point => new Tuple<int, int>(point[0], point[1])).ToList();
        }
    }
}