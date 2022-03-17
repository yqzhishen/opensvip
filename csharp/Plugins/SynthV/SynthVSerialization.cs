using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SynthV.Model;

namespace Plugin.SynthV
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

    public class SVNoteAttributesJsonConverter : JsonConverter<SVNoteAttributes>
    {
        public override void WriteJson(JsonWriter writer, SVNoteAttributes value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (!double.IsNaN(value.VibratoStart))
            {
                writer.WritePropertyName("tF0VbrStart");
                writer.WriteValue(value.VibratoStart);
            }
            if (!double.IsNaN(value.VibratoLeft))
            {
                writer.WritePropertyName("tF0VbrLeft");
                writer.WriteValue(value.VibratoLeft);
            }
            if (!double.IsNaN(value.VibratoRight))
            {
                writer.WritePropertyName("tF0VbrRight");
                writer.WriteValue(value.VibratoRight);
            }
            if (!double.IsNaN(value.VibratoDepth))
            {
                writer.WritePropertyName("dF0Vbr");
                writer.WriteValue(value.VibratoDepth);
            }
            if (!double.IsNaN(value.VibratoFrequency))
            {
                writer.WritePropertyName("fF0Vbr");
                writer.WriteValue(value.VibratoFrequency);
            }
            if (!double.IsNaN(value.VibratoPhase))
            {
                writer.WritePropertyName("pF0Vbr");
                writer.WriteValue(value.VibratoPhase);
            }
            if (!double.IsNaN(value.VibratoJitter))
            {
                writer.WritePropertyName("dF0Jitter");
                writer.WriteValue(value.VibratoJitter);
            }
            writer.WriteEndObject();
        }

        public override SVNoteAttributes ReadJson(JsonReader reader, Type objectType, SVNoteAttributes existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            return serializer.Deserialize<SVNoteAttributes>(reader);
        }
    }

}