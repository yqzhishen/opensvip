using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Model
{
    /// <summary>
    /// XS 3 拍号
    /// </summary>
    [ProtoContract]
    public class Xs3TimeSignature
    {
        /// <summary>
        /// 拍号内容
        /// </summary>
        [ProtoMember(2)]
        public Xs3TimeSignatureContent Content { get; set; }
    }

    /// <summary>
    /// XS 3 拍号内容
    /// </summary>
    [ProtoContract]
    public class Xs3TimeSignatureContent
    {
        /// <summary>
        /// 分子
        /// </summary>
        [ProtoMember(1)]
        public int Numerator { get; set; }

        /// <summary>
        /// 分母
        /// </summary>
        [ProtoMember(2)]
        public int Denominator { get; set; }
    }
}
