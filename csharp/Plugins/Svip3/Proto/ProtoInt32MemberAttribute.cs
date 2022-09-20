using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Proto
{
    public class ProtoInt32MemberAttribute : ProtoMemberAttribute
    {
        public ProtoInt32MemberAttribute(int tag) : base(tag)
        {
            DataFormat = DataFormat.ZigZag;
        }
    }
}
