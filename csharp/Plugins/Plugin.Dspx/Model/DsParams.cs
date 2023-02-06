using Newtonsoft.Json;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 单线条参数
    /// </summary>
    public class DsParams
    {
        [JsonProperty("pitch")]
        public DsParam Pitch { get; set; }

        [JsonProperty("energy")]
        public DsParam Energy { get; set; }
    }
}