using FlutyDeer.Svip3Plugin.Proto;
using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Model
{
    /// <summary>
    /// XS 3 参数点
    /// </summary>
    [ProtoContract]
    public class Xs3ParamPoint
    {
        /// <summary>
        /// 位置
        /// </summary>
        [ProtoMember(1, DataFormat = DataFormat.ZigZag)]
        public int Position { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [ProtoMember(2)]
        public float Value { get; set; }
    }
}