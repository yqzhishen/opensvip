using ProtoBuf;
using System.Collections.Generic;

namespace Plugin.Svip3
{
    [ProtoContract]
    public class SingAppModel
    {
        [ProtoMember(1)]
        public string ProjectFilePath { get; set; }

        [ProtoMember(2)]
        public string Version { get; set; }

        [ProtoMember(3)]
        public int Var1 { get; set; }

        [ProtoMember(4)]
        public PbTempo Tempo { get; set; }

        [ProtoMember(5)]
        public PbTimeSignatures TimeSignatures { get; set; }

        [ProtoMember(6)]
        public List<PbTrack> Tracks { get; set; } = new List<PbTrack>();

        /// <summary>
        /// 主音量（这里反序列化有问题）
        /// </summary>
        //[ProtoMember(7)]
        //public double MasterGain { get; set; }

        [ProtoMember(8)]
        public string Tonality { get; set; }

        [ProtoMember(9)]
        public int QuantizeValue { get; set; }

        [ProtoMember(10)]
        public int LoopStartPos { get; set; }

        [ProtoMember(11)]
        public int LoopEndPos { get; set; }

        [ProtoMember(12)]
        public bool Quantize { get; set; }

    }

    [ProtoContract]
    public class PbTrack
    {
        [ProtoMember(1)]
        public string ClassName { get; set; }

        [ProtoMember(2)]
        public PbTrackContent TrackContent { get; set; }
    }

    [ProtoContract]
    public class PbTrackContent
    {
        [ProtoMember(1)]
        public double Gain { get; set; }

        [ProtoMember(2)]
        private double _pan;

        public double Pan
        {
            get => _pan / 10.0;
            set => _pan = value * 10.0;
        }

        [ProtoMember(3)]
        public bool Mute { get; set; }

        [ProtoMember(4)]
        public string Name { get; set; }

        [ProtoMember(5)]
        public bool Solo { get; set; }

        [ProtoMember(6)]
        public string Color { get; set; }

        [ProtoMember(7)]
        public bool Bool1 { get; set; }

        [ProtoMember(8)]
        public List<PbPattern> PatternList { get; set; } = new List<PbPattern>();

        [ProtoMember(9)]
        public string UUID { get; set; }
    }

    [ProtoContract]
    //[ProtoInclude(8, typeof(PbSingingPattern))]
    //[ProtoInclude(8, typeof(PbAudioPattern))]
    public class PbPattern
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public int Int1 { get; set; }

        /// <summary>
        /// 样式原始起始位置
        /// </summary>
        [ProtoMember(3)]
        public int StartPosition { get; set; }

        /// <summary>
        /// 样式原始长度
        /// </summary>
        [ProtoMember(4)]
        public int Duration { get; set; }

        /// <summary>
        /// 样式从左侧裁剪位置（相对于样式起始位置）
        /// </summary>
        [ProtoMember(5)]
        public int ClipPosition { get; set; }

        /// <summary>
        /// 样式裁剪长度（样式裁剪后的实际长度）
        /// </summary>
        [ProtoMember(6)]
        public int ClippedDuration { get; set; }

        [ProtoMember(7)]
        public bool Mute { get; set; }

        [ProtoMember(8)]
        public List<PbNote> NoteList { get; set; }
    }

    //[ProtoContract]
    //public class PbSingingPattern : PbPattern
    //{
    //    [ProtoMember(8)]
    //    public List<PbNote> NoteList { get; set; }
    //}

    //[ProtoContract]
    //public class PbAudioPattern : PbPattern
    //{
    //    [ProtoMember(8)]
    //    public string AudioFilePath { get; set; }
    //}

    [ProtoContract]
    public class PbNote
    {
        /// <summary>
        /// 相对于样式的起始位置
        /// </summary>
        [ProtoMember(1)]
        public int RelativeStartPos { get; set; }

        [ProtoMember(2)]
        public int Duration { get; set; }

        [ProtoMember(3)]
        public int KeyIndex { get; set; }

        [ProtoMember(4)]
        public string Lyric { get; set; }

        [ProtoMember(5)]
        public string Pronunciation { get; set; }

        [ProtoMember(9)]
        public bool HeadTagSp { get; set; }

        [ProtoMember(10)]
        public bool HeadTagSil { get; set; }
    }

    [ProtoContract]
    public class PbTimeSignatures
    {
        [ProtoMember(2)]
        public PbTimeSignature TimeSignature { get; set; }
    }

    [ProtoContract]
    public class PbTimeSignature
    {
        [ProtoMember(1)]
        public int Numerator { get; set; }

        [ProtoMember(2)]
        public int Denominator { get; set; }
    }

    [ProtoContract]
    public class PbTempo
    {
        [ProtoMember(2)]
        private int bpm;

        public float BPM
        {
            get => bpm / 100.0f;
            set => bpm = (int)(value * 100);
        }
    }
}
