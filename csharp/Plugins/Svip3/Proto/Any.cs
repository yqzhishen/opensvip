using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Proto
{
    /// <summary>
    /// ProtoBuf Any 类型。
    /// </summary>
    /// <remarks>
    /// <para>可以通过静态类 <see cref="Any"/> 实现 Any 类型和 C# 对象的互转。</para>
    /// </remarks>
    [ProtoContract]
    public class Any
    {
        /// <summary>
        /// 类型 URL
        /// </summary>
        [ProtoMember(1)]
        public string TypeUrl { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [ProtoMember(2)]
        public byte[] Value { get; set; }

        /// <summary>
        /// 将对象打包成 Any 。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static Any Pack(object obj)
        {
            return new Any
            {
                TypeUrl = Constants.TypeUrlBase + obj.GetType().ToString(),
                Value = ProtoBufConvert.Serialize(obj)
            };
        }

        /// <summary>
        /// 将对象打包成 Any 。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static Any Pack(object obj, string type)
        {
            return new Any
            {
                TypeUrl = Constants.TypeUrlBase + type,
                Value = ProtoBufConvert.Serialize(obj)
            };
        }

        /// <summary>
        /// 从 Any 解包成对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="any">Any</param>
        /// <returns></returns>
        public static T Unpack<T>(Any any)
        {
            return ProtoBufConvert.Deserialize<T>(any.Value);
        }
    }
}
