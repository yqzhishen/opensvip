using Newtonsoft.Json;
using System.Collections.Generic;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 时间轴
    /// </summary>
    public class DsTimeline
    {
        [JsonProperty("timeSignatures")]
        public List<DsTimeSignature> TimeSignatures { get; set; }

        [JsonProperty("tempos")]
        public List<DsTempo> Tempos { get; set; }

        [JsonProperty("labels")]
        public List<DsLabel> Labels { get; set; }
    }

    /// <summary>
    /// 拍号
    /// </summary>
    public class DsTimeSignature
    {
        [JsonProperty("pos")]
        public int Position { get; set; }

        [JsonProperty("numerator")]
        public int Numerator { get; set; }

        [JsonProperty("denominator")]
        public int Denominator { get; set; }
    }

    /// <summary>
    /// 曲速
    /// </summary>
    public class DsTempo
    {
        [JsonProperty("pos")]
        public int Position { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }

    /// <summary>
    /// 标签
    /// </summary>
    public class DsLabel
    {
        [JsonProperty("pos")]
        public int Position { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}