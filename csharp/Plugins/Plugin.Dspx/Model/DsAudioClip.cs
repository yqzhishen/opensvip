using Newtonsoft.Json;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 波形剪辑
    /// </summary>
    public class DsAudioClip : DsClip
    {
        [JsonProperty("path")]
        public string AudioFilePath { get; set; }

        public DsAudioClip() : base(DsClipType.Audio)
        {
        }
    }
}