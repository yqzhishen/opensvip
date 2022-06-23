using FlutyDeer.GjgjPlugin.Model;
using OpenSvip.Model;
using System.Collections.Generic;

namespace FlutyDeer.GjgjPlugin.Utils
{
    public static class TimeSignatureUtil
    {
        public static List<TimeSignature> OsTimeSignatureList { get; set; }

        public static int GetFirstBarLength()
        {
            return 1920 * GetNumerator(0) / GetDenominator(0);
        }

        /// <summary>
        /// 返回转换后的拍号。
        /// </summary>
        /// <param name="sumOfTime">从谱面开始到当前拍号的累计时间。</param>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        public static GjTimeSignature EncodeTimeSignature(ref int sumOfTime, int index)
        {
            GjTimeSignature gjTimeSignature = new GjTimeSignature
            {
                Time = GetTimeSignatureTime(ref sumOfTime, index),
                Numerator = GetNumerator(index),
                Denominator = GetDenominator(index)
            };
            return gjTimeSignature;
        }

        /// <summary>
        /// 返回转换后的拍号的时间。
        /// </summary>
        /// <param name="sumOfTime">从谱面开始到当前拍号的累计时间。</param>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        public static int GetTimeSignatureTime(ref int sumOfTime, int index)
        {
            int time;
            if (index == 0)
            {
                time = 0;
            }
            else
            {
                sumOfTime += (GetBarIndex(index) - GetBarIndex(index - 1)) * 1920 * GetNumerator(index - 1) / GetDenominator(index - 1);
                time = sumOfTime;
            }
            return time;
        }

        /// <summary>
        /// 返回拍号所在的小节。
        /// </summary>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        public static int GetBarIndex(int index)
        {
            return OsTimeSignatureList[index].BarIndex;
        }

        /// <summary>
        /// 返回拍号的分子。
        /// </summary>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        public static int GetNumerator(int index)
        {
            return OsTimeSignatureList[index].Numerator;
        }

        /// <summary>
        /// 返回拍号的分母。
        /// </summary>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        private static int GetDenominator(int index)
        {
            return OsTimeSignatureList[index].Denominator;
        }
    }
}
