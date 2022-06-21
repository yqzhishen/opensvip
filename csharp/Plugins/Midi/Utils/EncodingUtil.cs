using System.Text;
using FlutyDeer.MidiPlugin.Options;

namespace FlutyDeer.MidiPlugin.Utils
{
    public static class EncodingUtil
    {
        /// <summary>
        /// 将选项转换为编码。
        /// </summary>
        /// <returns>文本编码。</returns>
        public static Encoding GetEncoding(LyricEncodingOption LyricEncoding)
        {
            switch (LyricEncoding)
            {
                case LyricEncodingOption.ASCII:
                    return Encoding.ASCII;
                case LyricEncodingOption.BigEndianUnicode:
                    return Encoding.BigEndianUnicode;
                case LyricEncodingOption.Default:
                    return Encoding.Default;
                case LyricEncodingOption.Unicode:
                    return Encoding.Unicode;
                case LyricEncodingOption.UTF32:
                    return Encoding.UTF32;
                case LyricEncodingOption.UTF7:
                    return Encoding.UTF7;
                case LyricEncodingOption.UTF8BOM:
                    return Encoding.UTF8;
                case LyricEncodingOption.UTF8:
                    return new UTF8Encoding(false);//不带BOM的UTF-8
                default:
                    return Encoding.UTF8;
            }
        }
    }
}