using ProtoBuf;
using System.IO;

namespace FlutyDeer.Svip3Plugin.Proto
{
    /// <summary>
    /// 提供 C# 对象和 ProtoBuf 互转的方法。
    /// </summary>
    public static class ProtoBufConvert
    {
        /// <summary>
        /// 反序列化 ProtoBuf 到指定 C# 类型。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] value)
        {
            T obj;
            using (var stream = new MemoryStream(value))
            {
                obj = Serializer.Deserialize<T>(stream);
            }
            return obj;
        }

        /// <summary>
        /// 序列化 C# 对象到 ProtoBuf 。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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
