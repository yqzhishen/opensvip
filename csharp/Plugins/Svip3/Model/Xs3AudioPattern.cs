using ProtoBuf;
using System.Collections.Generic;

namespace FlutyDeer.Svip3Plugin.Model
{
    [ProtoContract]
    public class Xs3AudioPattern
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// 样式原始起始位置（被裁剪之前的位置）
        /// </summary>
        [ProtoMember(3)]
        public int OriginalStartPosition { get; set; }

        /// <summary>
        /// 样式原始长度（被裁剪前的长度，即音频本身的长度）
        /// </summary>
        [ProtoMember(4)]
        public int OriginalDuration { get; set; }

        /// <summary>
        /// 样式裁剪位置（相对于样式起始位置）
        /// </summary>
        [ProtoMember(5)]
        public int ClipPosition { get; set; }

        /// <summary>
        /// 样式裁剪后长度（被裁剪后可见部分的长度）
        /// </summary>
        [ProtoMember(6)]
        public int ClippedDuration { get; set; }

        [ProtoMember(7)]
        public bool Mute { get; set; }

        [ProtoMember(8)]
        public string AudioFilePath { get; set; }
    }
}
