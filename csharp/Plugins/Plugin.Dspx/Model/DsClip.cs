using Newtonsoft.Json;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// 音轨区间
    /// </summary>
    public class DsClip
    {
        #region Properties

        [JsonProperty("type")]
        public DsClipType Type { get; set; }

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

        protected DsClip(DsClipType type)
        {
            Type = type;
        }

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
        [JsonProperty("audio")]
        Audio,

        [JsonProperty("singing")]
        Singing
    }
}