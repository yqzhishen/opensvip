using System;

namespace Json2DiffSinger.Utils
{
    /// <summary>
    /// MIDI 音高和频率互转工具。
    /// </summary>
    public static class ToneUtils
    {
        /// <summary>
        /// 音高转频率
        /// </summary>
        /// <param name="tone">音高</param>
        /// <returns></returns>
        public static float ToneToFreq(float tone)
        {
            return (float)(440.0 * Math.Pow(2, (tone - 69.0) / 12.0));
        }

        /// <summary>
        /// 频率转音高
        /// </summary>
        /// <param name="freq">频率</param>
        /// <returns></returns>
        public static float FreqToTone(float freq)
        {
            return (float)(69.0 + 12.0 * Math.Log(freq / 440.0, 2));
        }
    }
}
