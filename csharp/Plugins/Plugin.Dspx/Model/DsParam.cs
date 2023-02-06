using Newtonsoft.Json;
using System.Collections.Generic;

namespace Plugin.Dspx.Model
{
    public class DsParam
    {
        [JsonProperty("original")]
        public List<DsParamCurve> Original { get; set; }

        [JsonProperty("edited")]
        public List<DsParamCurve> Edited { get; set; }

        [JsonProperty("envelope")]
        public List<DsParamCurve> Envelope { get; set; }
    }

    public class DsParamCurve
    {
        [JsonProperty("type")]
        public DsParamType Type { get; set; }

        protected DsParamCurve(DsParamType type)
        {
            Type = type;
        }
    }

    public class DsParamFree : DsParamCurve
    {
        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("step")]
        public int StepSize { get; set; }

        [JsonProperty("values")]
        public List<int> Values { get; set; }

        public DsParamFree() : base(DsParamType.Free)
        {
        }
    }

    public class DsParamAnchor : DsParamCurve
    {
        [JsonProperty("nodes")]
        public List<DsParamNode> Nodes { get; set; }

        public DsParamAnchor() : base(DsParamType.Anchor)
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
        public DsInterpolationType Type { get; set; }
    }

    public enum DsParamType
    {
        [JsonProperty("free")]
        Free,

        [JsonProperty("anchor")]
        Anchor
    }

    public enum DsInterpolationType
    {
        [JsonProperty("linear")]
        Linear,

        [JsonProperty("hermite")]
        Hermite
    }
}