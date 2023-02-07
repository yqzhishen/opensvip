using Newtonsoft.Json;
using System.Collections.Generic;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 工程可编辑区域
    /// </summary>
    public class DsContent
    {
        [JsonProperty("master")]
        public DsMaster Master { get; set; }

        [JsonProperty("timeline")]
        public DsTimeline Timeline { get; set; }

        [JsonProperty("tracks")]
        public List<DsTrack> Tracks { get; set; }

        [JsonProperty("extra")]
        public DsExtra Extra { get; set; }

        [JsonProperty("workspace")]
        public DsWorkspace Workspace { get; set; }
    }
}