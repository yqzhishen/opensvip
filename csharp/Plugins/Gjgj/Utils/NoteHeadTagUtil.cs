namespace FlutyDeer.GjgjPlugin.Utils
{
    public static class NoteHeadTagUtil
    {
        /// <summary>
        /// 转换音符标记（无、换气和停顿）。
        /// </summary>
        /// <returns></returns>
        public static string ToStringTag(int style)
        {
            switch (style)
            {
                case 0:
                    return null;
                case 1:
                    return "V";
                case 2:
                    return "0";
                default:
                    return null;
            }
        }

        /// <summary>
        /// 返回音符标记（无、换气和停顿）。
        /// </summary>
        /// <param name="origin">原始音符标记。</param>
        /// <returns></returns>
        public static int ToIntTag(string origin)
        {
            switch (origin)
            {
                case null:
                    return 0;
                case "V":
                    return 1;
                case "0":
                    return 2;
                default:
                    return 0;
            }
        }
    }
}