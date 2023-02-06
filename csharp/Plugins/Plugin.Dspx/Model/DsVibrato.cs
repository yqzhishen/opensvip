using Newtonsoft.Json;
using System.Collections.Generic;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 颤音
    /// </summary>
    public class DsVibrato
    {
        [JsonProperty("start")]
        public double Start { get; set; }

        [JsonProperty("end")]
        public double End { get; set; }

        [JsonProperty("freq")]
        public double Frequency { get; set; }

        [JsonProperty("phase")]
        public double Phase { get; set; }

        [JsonProperty("amp")]
        public double Amplitude { get; set; }

        [JsonProperty("offset")]
        public double Offset { get; set; }

        [JsonProperty("points")]
        public List<DsVibratoPoint> Points { get; set; }
    }

    public class DsVibratoPoint
    {
        [JsonProperty("x")]
        public double LengthRatio { get; set; }

        [JsonProperty("y")]
        public double AmplitudeRatio { get; set; }
    }
}