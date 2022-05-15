using Gjgj.Model;
using OpenSvip.Model;

namespace Plugin.Gjgj
{
    public static class TempoUtil
    {
        /// <summary>
        /// 返回每四分音符的微秒数。
        /// </summary>
        /// <param name="BPM">BPM.</param>
        /// <returns></returns>
        public static int ToMicrosecondsPerQuarterNote(float BPM)
        {
            return (int)(60.0 / BPM * 1000000.0);
        }
        
        /// <summary>
        /// 返回曲速标记的BPM。
        /// </summary>
        /// <param name="microsecondsPerQuarterNote">每四分音符的微秒数。</param>
        /// <returns></returns>
        public static float ToBPM(int microsecondsPerQuarterNote)
        {
            double origin = microsecondsPerQuarterNote;
            return (float)(60.0 / origin * 1000000.0);
        }

        /// <summary>
        /// 转换曲速标记。
        /// </summary>
        /// <returns></returns>
        public static SongTempo DecodeSongTempo(GjTempo gjTempo)
        {
            SongTempo songTempo = new SongTempo
            {
                Position = gjTempo.Time,
                BPM = ToBPM(gjTempo.MicrosecondsPerQuarterNote)
            };
            return songTempo;
        }
        
        /// <summary>
        /// 返回转换后的曲速标记。
        /// </summary>
        /// <returns></returns>
        public static GjTempo EncodeTempo(SongTempo songTempo)
        {
            GjTempo gjTempo = new GjTempo
            {
                Time = songTempo.Position,
                MicrosecondsPerQuarterNote = ToMicrosecondsPerQuarterNote(songTempo.BPM)
            };
            return gjTempo;
        }
    }
}