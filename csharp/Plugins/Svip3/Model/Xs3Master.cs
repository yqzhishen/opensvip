using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Model
{
    /// <summary>
    /// XS 3 总线
    /// </summary>
    [ProtoContract]
    public class Xs3Master
    {
        /// <summary>
        /// 音量
        /// </summary>
        [ProtoMember(1)]
        public float Gain { get; set; }
    }
}