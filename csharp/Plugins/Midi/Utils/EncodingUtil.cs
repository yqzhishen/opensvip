using System.Text;

namespace FlutyDeer.MidiPlugin
{
    public static class EncodingUtil
    {
        /// <summary>
        /// 将选项转换为编码。
        /// </summary>
        /// <returns>文本编码。</returns>
        public static Encoding GetEncoding(LyricEncodings LyricEncoding)
        {
            switch (LyricEncoding)
            {
                case LyricEncodings.ASCII:
                    return Encoding.ASCII;
                case LyricEncodings.BigEndianUnicode:
                    return Encoding.BigEndianUnicode;
                case LyricEncodings.Default:
                    return Encoding.Default;
                case LyricEncodings.Unicode:
                    return Encoding.Unicode;
                case LyricEncodings.UTF32:
                    return Encoding.UTF32;
                case LyricEncodings.UTF7:
                    return Encoding.UTF7;
                case LyricEncodings.UTF8BOM:
                    return Encoding.UTF8;
                case LyricEncodings.UTF8:
                    return new UTF8Encoding(false);//不带BOM的UTF-8
                default:
                    return Encoding.UTF8;
            }
        }
    }
}