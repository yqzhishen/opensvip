using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Model
{
    [ProtoContract]
    public class Xs3ParamPoint
    {
        [ProtoMember(1)]
        public int Position { get; set; }

        [ProtoMember(2)]
        public int Value { get; set; }
    }
}