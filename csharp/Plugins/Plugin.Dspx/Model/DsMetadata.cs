using Newtonsoft.Json;
using System;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 文件的元信息，包括版本号、工程名、作者等
    /// </summary>
    public class DsMetadata
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }
    }
}
