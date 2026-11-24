using Newtonsoft.Json;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 波形区间
    /// </summary>
    public class DsAudioClip : DsClip
    {
        [JsonProperty("path")]
        public string AudioFilePath { get; set; }

        //public DsAudioClip() : base(DsClipType.Audio)
        public DsAudioClip() : base("audio")
        {
        }
    }
}