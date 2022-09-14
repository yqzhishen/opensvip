using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Model
{
    [ProtoContract]
    public class Xs3Master
    {
        [ProtoMember(1)]
        public float Gain { get; set; }
    }
}