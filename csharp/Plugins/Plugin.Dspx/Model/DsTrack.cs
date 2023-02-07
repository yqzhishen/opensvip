using Newtonsoft.Json;
using System.Collections.Generic;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 音轨列表
    /// </summary>
    public class DsTrack
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("control")]
        public DsTrackControl Control { get; set; }

        [JsonProperty("clips")]
        [JsonConverter(typeof(ClipJsonConverter))]
        public List<DsClip> Clips { get; set; }

        [JsonProperty("extra")]
        public DsExtra Extra { get; set; }

        [JsonProperty("workspace")]
        public DsWorkspace Workspace { get; set; }
    }

    /// <summary>
    /// 音轨主控
    /// </summary>
    public class DsTrackControl
    {
        [JsonProperty("gain")]
        public double Gain { get; set; }

        [JsonProperty("pan")]
        public double Pan { get; set; }

        [JsonProperty("mute")]
        public bool Mute { get; set; }

        [JsonProperty("solo")]
        public bool Solo { get; set; }
    }
}