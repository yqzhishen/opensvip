using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Model
{
    [ProtoContract]
    public class Xs3Note
    {
        /// <summary>
        /// 相对于样式的起始位置
        /// </summary>
        [ProtoMember(1)]
        public int StartPosition { get; set; }

        [ProtoMember(2)]
        public int Duration { get; set; }

        [ProtoMember(3)]
        public int KeyIndex { get; set; }

        [ProtoMember(4)]
        public string Lyric { get; set; }

        [ProtoMember(5)]
        public string Pronunciation { get; set; }

        [ProtoMember(6)]
        public int ConsonantLength { get; set; }

        [ProtoMember(9)]
        public int SpLength { get; set; }

        [ProtoMember(10)]
        public int SilLength { get; set; }
    }
}
