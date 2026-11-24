using Newtonsoft.Json;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 总线控制
    /// </summary>
    public class DsMaster
    {
        [JsonProperty("control")]
        public DsControl Control { get; set; }

        [JsonProperty("loop")]
        public DsLoop Loop { get; set; }
    }

    /// <summary>
    /// 循环区间
    /// </summary>
    public class DsLoop
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }
    }
}