using Newtonsoft.Json;
using System.ComponentModel;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 音轨区间
    /// </summary>
    //[JsonConverter(typeof(ClipJsonConverter))]
    public class DsClip
    {
        #region Properties

        //[JsonConverter(typeof(StringEnumConverter))]
        //[JsonIgnore]
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("time")]
        public DsTime Time { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("control")]
        public DsControl Control { get; set; }

        [JsonProperty("extra")]
        public DsExtra Extra { get; set; }

        [JsonProperty("workspace")]
        public DsWorkspace Workspace { get; set; }

        #endregion Properties

        #region Constructors

        protected DsClip(string type) => Type = type;
        //protected DsClip(DsClipType type) => Type = type;

        #endregion Constructors
    }

    /// <summary>
    /// 时间信息
    /// </summary>
    public class DsTime
    {
        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("clipStart")]
        public int ClipStart { get; set; }

        [JsonProperty("clipLen")]
        public int ClipLength { get; set; }
    }

    /// <summary>
    /// 类型
    /// </summary>
    public enum DsClipType
    {
        [Description("audio")]
        Audio,

        [Description("singing")]
        Singing
    }
}