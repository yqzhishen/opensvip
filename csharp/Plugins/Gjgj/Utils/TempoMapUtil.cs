using FlutyDeer.GjgjPlugin.Model;
using OpenSvip.Model;
using System.Collections.Generic;

namespace FlutyDeer.GjgjPlugin.Utils
{
    public static class TempoMapUtil
    {
        /// <summary>
        /// 返回谱面的曲速和拍号信息。
        /// </summary>
        /// <returns></returns>
        public static GjTempoMap EncodeTempoMap(List<SongTempo> songTempoList, List<TimeSignature> timeSignatureList)
        {
            GjTempoMap gjTempoMap = new GjTempoMap
            {
                TicksPerQuarterNote = 480,
                TempoList = EncodeTempoList(songTempoList),
                TimeSignatureList = EncodeTimeSignatureList(timeSignatureList)
            };
            return gjTempoMap;
        }

        /// <summary>
        /// 返回转换后的曲速列表。
        /// </summary>
        /// <returns></returns>
        private static List<GjTempo> EncodeTempoList(List<SongTempo> songTempoList)
        {
            List<GjTempo> gjTempoList = new List<GjTempo>();
            foreach (var tempo in songTempoList)
            {
                gjTempoList.Add(TempoUtil.EncodeTempo(tempo));
            }
            return gjTempoList;
        }

        /// <summary>
        /// 返回转换后的拍号列表。
        /// </summary>
        /// <returns></returns>
        private static List<GjTimeSignature> EncodeTimeSignatureList(List<TimeSignature> timeSignatureList)
        {
            List<GjTimeSignature> gjTimeSignatureList = new List<GjTimeSignature>(timeSignatureList.Count);
            int sumOfTime = 0;
            for (int index = 0; index < timeSignatureList.Count; index++)
            {
                gjTimeSignatureList.Add(TimeSignatureUtil.EncodeTimeSignature(ref sumOfTime, index));
            }
            return gjTimeSignatureList;
        }
    }
}
