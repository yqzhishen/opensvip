using ProtoBuf;
using System.Collections.Generic;

namespace FlutyDeer.Svip3Plugin.Model
{
    /// <summary>
    /// XS 3 演唱轨
    /// </summary>
    [ProtoContract]
    public class Xs3SingingTrack
    {
        /// <summary>
        /// 音量
        /// </summary>
        [ProtoMember(1)]
        public float Gain { get; set; }

        /// <summary>
        /// 声像
        /// </summary>
        [ProtoMember(2)]
        public float Pan { get; set; }

        /// <summary>
        /// 静音
        /// </summary>
        [ProtoMember(3)]
        public bool Mute { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [ProtoMember(4)]
        public string Name { get; set; }

        /// <summary>
        /// 独奏
        /// </summary>
        [ProtoMember(5)]
        public bool Solo { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        [ProtoMember(6)]
        public string Color { get; set; }

        /// <summary>
        /// 样式列表
        /// </summary>
        [ProtoMember(8)]
        public List<Xs3SingingPattern> PatternList { get; set; } = new List<Xs3SingingPattern>();

        /// <summary>
        /// 歌手 ID
        /// </summary>
        [ProtoMember(9)]
        public string SingerId { get; set; }

    }
}
