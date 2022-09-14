using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Model
{
    [ProtoContract]
    public class Xs3TimeSignature
    {
        [ProtoMember(2)]
        public Xs3TimeSignatureContent Content { get; set; }
    }

    [ProtoContract]
    public class Xs3TimeSignatureContent
    {
        [ProtoMember(1)]
        public int Numerator { get; set; }

        [ProtoMember(2)]
        public int Denominator { get; set; }
    }
}
