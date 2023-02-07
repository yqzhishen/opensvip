using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Plugin.Dspx.Model
{
    public class ParamCurveJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jArray = serializer.Deserialize<JArray>(reader);
            var curves = new List<DsParamCurve>();
            foreach (var jToken in jArray)
            {
                if (jToken == null)
                    return null;
                var type = jToken.Value<string>("type");
                switch (type)
                {
                    case "free":
                        curves.Add(jToken.ToObject<DsParamFree>());
                        break;
                    case "anchor":
                        curves.Add(jToken.ToObject<DsParamAnchor>());
                        break;
                    default:
                        break;
                }
            }
            return curves;
            //var jobj = serializer.Deserialize<JObject>(reader);
            //if (jobj == null)
            //    return null;
            //var type = jobj.Value<string>("type");
            //switch (type)
            //{
            //    case "free":
            //        return new DsParamFree
            //        {
            //            Start = jobj.Value<int>("start"),
            //            StepSize = jobj.Value<int>("step"),
            //            Values = jobj.Value<JArray>("values")?.ToObject<List<int>>()
            //        };

            //    case "anchor":
            //        return new DsParamAnchor
            //        {
            //            Nodes = jobj.Value<JArray>("nodes")?.ToObject<List<DsParamNode>>()
            //        };

            //    default:
            //        return null;
            //}
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}