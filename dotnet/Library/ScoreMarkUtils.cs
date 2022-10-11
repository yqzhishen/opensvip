using System.Collections.Generic;
using System.Linq;
using OpenSvip.Model;

namespace OpenSvip.Library
{
    /// <summary>
    /// 处理曲速和拍号的工具类。
    /// </summary>
    public static class ScoreMarkUtils
    {
        /// <summary>
        /// 给定曲速列表从前端截去一定长度，并返回以截断处为零点的新曲速列表。
        /// </summary>
        /// <param name="tempoList">原曲速列表</param>
        /// <param name="skipTicks">截断长度（梯）</param>
        public static List<SongTempo> SkipTempoList(List<SongTempo> tempoList, int skipTicks)
        {
            var result = tempoList
                .Where(tempo => tempo.Position >= skipTicks)
                .Select(
                    tempo => new SongTempo
                    {
                        Position = tempo.Position - skipTicks,
                        BPM = tempo.BPM
                    }).ToList();
            if (result.Any() && result[0].Position <= 0)
            {
                return result;
            }

            var i = 0;
            for (; i < tempoList.Count && tempoList[i].Position <= skipTicks; i++)
            {
            }

            result.Insert(0, new SongTempo
            {
                Position = 0,
                BPM = tempoList[i - 1].BPM
            });
            return result;
        }

        /// <summary>
        /// 截断给定拍号列表前面的若干个小节，并返回以截断处为零点的新拍号列表。
        /// </summary>
        /// <param name="beatList">原拍号列表</param>
        /// <param name="skipBars">截断的小节数</param>
        public static List<TimeSignature> SkipBeatList(List<TimeSignature> beatList, int skipBars)
        {
            var result = beatList
                .Where(beat => beat.BarIndex >= skipBars)
                .Select(
                    meter => new TimeSignature
                    {
                        BarIndex = meter.BarIndex - skipBars,
                        Numerator = meter.Numerator,
                        Denominator = meter.Denominator
                    }).ToList();
            if (!result.Any() || result[0].BarIndex > 0)
            {
                result.Insert(0, new TimeSignature
                {
                    BarIndex = 0,
                    Numerator = beatList[0].Numerator,
                    Denominator = beatList[0].Denominator
                });
            }
            return result;
        }

        public static List<SongTempo> ShiftTempoList(List<SongTempo> tempoList, int shiftTicks)
        {
            var result = new List<SongTempo>
            {
                tempoList[0]
            };
            result.AddRange(tempoList
                .Skip(1)
                .Select(tempo => new SongTempo
                {
                    Position = tempo.Position + shiftTicks,
                    BPM = tempo.BPM
                }));
            return result;
        }

        public static List<TimeSignature> ShiftBeatList(List<TimeSignature> beatList, int shiftBars)
        {
            var result = new List<TimeSignature>
            {
                beatList[0]
            };
            result.AddRange(beatList
                .Skip(1)
                .Select(beat => new TimeSignature
                {
                    BarIndex = beat.BarIndex + shiftBars,
                    Numerator = beat.Numerator,
                    Denominator = beat.Denominator
                }));
            return result;
        }
    }
}