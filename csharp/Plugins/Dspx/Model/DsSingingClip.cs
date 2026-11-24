using Newtonsoft.Json;
using System.Collections.Generic;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 人声区间
    /// </summary>
    public class DsSingingClip : DsClip
    {
        [JsonProperty("notes")]
        public List<DsNote> Notes { get; set; }

        [JsonProperty("params")]
        public DsParams Params { get; set; }

        [JsonProperty("sources")]
        public DsSources Sources { get; set; }

        //public DsSingingClip() : base(DsClipType.Singing)
        public DsSingingClip() : base("singing")
        {
        }
    }
}