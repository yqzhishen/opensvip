using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Proto
{
    [ProtoContract]
    public class Any
    {
        [ProtoMember(1)]
        public string TypeUrl { get; set; }

        [ProtoMember(2)]
        public byte[] Value { get; set; }

        public static Any Pack(object obj)
        {
            return new Any
            {
                TypeUrl = Constants.TypeUrlBase + obj.GetType().ToString(),
                Value = ProtoBufConvert.Serialize(obj)
            };
        }

        public static Any Pack(object obj, string type)
        {
            return new Any
            {
                TypeUrl = Constants.TypeUrlBase + type,
                Value = ProtoBufConvert.Serialize(obj)
            };
        }

        public static T Unpack<T>(Any any)
        {
            return ProtoBufConvert.Deserialize<T>(any.Value);
        }
    }
}
