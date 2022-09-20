using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Model
{
    /// <summary>
    /// XS 3 音符
    /// </summary>
    [ProtoContract]
    public class Xs3Note
    {
        /// <summary>
        /// 相对于样式的起始位置
        /// </summary>
        [ProtoMember(1)]
        public int StartPosition { get; set; }

        /// <summary>
        /// 时长
        /// </summary>
        [ProtoMember(2)]
        public int Duration { get; set; }

        /// <summary>
        /// 音高
        /// </summary>
        [ProtoMember(3)]
        public int KeyIndex { get; set; }

        /// <summary>
        /// 歌词
        /// </summary>
        [ProtoMember(4)]
        public string Lyric { get; set; }

        /// <summary>
        /// 发音
        /// </summary>
        [ProtoMember(5)]
        public string Pronunciation { get; set; }

        /// <summary>
        /// 辅音长度
        /// </summary>
        [ProtoMember(6)]
        public int ConsonantLength { get; set; }

        [ProtoMember(7)]
        public bool HasConsonant { get; set; }

        /// <summary>
        /// 换气长度
        /// </summary>
        [ProtoMember(9)]
        public int SpLength { get; set; }

        /// <summary>
        /// 停顿长度
        /// </summary>
        [ProtoMember(10)]
        public int SilLength { get; set; }
    }
}
