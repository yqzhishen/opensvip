using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Plugin.Dspx.Model
{
    public class DsParam
    {
        [JsonProperty("original")]
        [JsonConverter(typeof(ParamCurveJsonConverter))]
        public List<DsParamCurve> Original { get; set; }

        [JsonProperty("edited")]
        [JsonConverter(typeof(ParamCurveJsonConverter))]
        public List<DsParamCurve> Edited { get; set; }

        [JsonProperty("envelope")]
        [JsonConverter(typeof(ParamCurveJsonConverter))]
        public List<DsParamCurve> Envelope { get; set; }
    }

    public class DsParamCurve
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        //[JsonConverter(typeof(StringEnumConverter))]
        //public DsParamType Type { get; set; }

        protected DsParamCurve(string type) => Type = type;
    }

    public class DsParamFree : DsParamCurve
    {
        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("step")]
        public int StepSize { get; set; }

        [JsonProperty("values")]
        public List<int> Values { get; set; }

        //public DsParamFree() : base(DsParamType.Free)
        public DsParamFree() : base("free")
        {
        }
    }

    public class DsParamAnchor : DsParamCurve
    {
        [JsonProperty("nodes")]
        public List<DsParamNode> Nodes { get; set; }

        //public DsParamAnchor() : base(DsParamType.Anchor)
        public DsParamAnchor() : base("anchor")
        {
        }
    }

    public class DsParamNode
    {
        [JsonProperty("x")]
        public int Time { get; set; }

        [JsonProperty("y")]
        public int Value { get; set; }

        [JsonProperty("interp")]
        public string Type { get; set; }
        //public DsInterpolationType? Type { get; set; }
    }

    public enum DsParamType
    {
        [Description("free")]
        Free,

        [Description("anchor")]
        Anchor
    }

    public enum DsInterpolationType
    {
        [Description("linear")]
        Linear,

        [Description("hermite")]
        Hermite
    }
}