using ProtoBuf;
using System.IO;

namespace FlutyDeer.Svip3Plugin.Proto
{
    public static class ProtoBufConvert
    {

        public static T Deserialize<T>(byte[] value)
        {
            T obj;
            using (var stream = new MemoryStream(value))
            {
                obj = Serializer.Deserialize<T>(stream);
            }
            return obj;
        }

        public static byte[] Serialize(object obj)
        {
            byte[] value;
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, obj);
                value = stream.ToArray();
            }
            return value;
        }
    }
}
