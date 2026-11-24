using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 音素列表
    /// </summary>
    public class DsPhonemes
    {
        /// <summary>
        /// 自动参数列表
        /// </summary>
        [JsonProperty("original")]
        public List<DsPhoneme> Original { get; set; }

        /// <summary>
        /// 已修改的参数列表
        /// </summary>
        [JsonProperty("edited")]
        public List<DsPhoneme> Edited { get; set; }

    }

    /// <summary>
    /// 音素
    /// </summary>
    public class DsPhoneme
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        //public DsPhonemeType Type { get; set; }

        [JsonProperty("token")]
        public string Name { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("extra")]
        public DsExtra Extra { get; set; }

        [JsonProperty("workspace")]
        public DsWorkspace Workspace { get; set; }
    }

    /// <summary>
    /// 音素类型
    /// </summary>
    public enum DsPhonemeType
    {
        [Description("ahead")]
        Ahead,

        [Description("normal")]
        Normal,

        [Description("final")]
        Final
    }
}
