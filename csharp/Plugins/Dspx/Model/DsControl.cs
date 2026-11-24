using Newtonsoft.Json;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 主控
    /// </summary>
    public class DsControl
    {
        [JsonProperty("gain")]
        public double Gain { get; set; }

        [JsonProperty("mute")]
        public bool Mute { get; set; }
    }
}