using System.Collections.Generic;
using System.Linq;
using OpenSvip.Model;

namespace OpenSvip.Library
{
    /// <summary>
    /// 曲谱时间同步器，用于对齐和转换时间轴，将原始谱面上的音符位置转换为对齐后的实际位置坐标或时间坐标。
    /// </summary>
    public class TimeSynchronizer
    {
        /// <summary>
        /// 经过 ignoredTicks 偏移后的曲速列表。若 ignoredTicks == 0，此列表和传入的曲速列表是一致的。
        /// </summary>
        public List<SongTempo> TempoList { get; }

        private readonly bool IsAbsoluteTimeMode;

        private readonly int DefaultTempo;

        /// <summary>
        /// 实例化一个新的曲谱时间同步器。通常每个工程文件只需要使用一个时间同步器。
        /// </summary>
        /// <param name="originalTempoList">该曲谱的原始曲速列表。</param>
        /// <param name="skipTicks">
        ///     指定原始谱面上需要跳过的开头长度，单位为梯。
        ///     例如 OpenSvip Model 中曲速位置的零点比音符位置的零点靠前一个小节，需将第一小节的长度设置为忽略。</param>
        /// <param name="isAbsoluteTimeMode">是否采用绝对时间对齐模式。
        ///     当出现不支持的拍号、曲速等情况时可以开启此模式，将使用恒定曲速进行绝对时间的对齐。</param>
        /// <param name="defaultTempo">当采用绝对时间对齐模式时，可以指定用于对齐的默认曲速。</param>
        public TimeSynchronizer(List<SongTempo> originalTempoList,
            int skipTicks = 0,
            bool isAbsoluteTimeMode = false,
            int defaultTempo = 60)
        {
            TempoList = skipTicks > 0 ? ScoreMarkUtils.SkipTempoList(originalTempoList, skipTicks) : originalTempoList;
            IsAbsoluteTimeMode = isAbsoluteTimeMode;
            DefaultTempo = defaultTempo;
        }

        /// <summary>
        /// 将原始谱面位置（梯）转换为对齐后的实际谱面位置（梯）。
        /// </summary>
        public double GetActualTicksFromTicks(int ticks)
        {
            if (!IsAbsoluteTimeMode)
            {
                return ticks;
            }
            var res = 0.0;
            var i = 0;
            for (; i < TempoList.Count - 1 && TempoList[i + 1].Position < ticks; i++)
            {
                res += (TempoList[i + 1].Position - TempoList[i].Position) * DefaultTempo / TempoList[i].BPM;
            }
            res += (ticks - TempoList[i].Position) * DefaultTempo / TempoList[i].BPM;
            return res;
        }

        /// <summary>
        /// 将原始谱面位置（梯）转换为对齐后的实际谱面时间坐标（秒）。
        /// </summary>
        public double GetActualSecsFromTicks(int ticks)
        {
            return GetDurationSecsFromTicks(0, ticks);
        }

        /// <summary>
        /// 将原始谱面时间坐标（秒）转换为对齐后的实际谱面位置（梯）。
        /// </summary>
        public double GetActualTicksFromSecs(double secs)
        {
            return GetActualTicksFromSecsOffset(0, secs);
        }

        /// <summary>
        /// 计算原始谱面上两个位置坐标（梯）在对齐后的时间差（秒）。
        /// </summary>
        public double GetDurationSecsFromTicks(int startTicks, int endTicks)
        {
            if (IsAbsoluteTimeMode)
            {
                return (GetActualTicksFromTicks(endTicks) - GetActualTicksFromTicks(startTicks)) / DefaultTempo / 8;
            }
            
            var startTempoIndex = TempoList.FindLastIndex(tempo => tempo.Position <= startTicks);
            var endTempoIndex = TempoList.FindLastIndex(tempo => tempo.Position <= endTicks);
            
            if (startTempoIndex == endTempoIndex)
            {
                return (endTicks - startTicks) / TempoList[startTempoIndex].BPM / 8;
            }
            
            var secs = 0.0;
            secs += (TempoList[startTempoIndex + 1].Position - startTicks)
                    / (double) TempoList[startTempoIndex].BPM / 8;
            for (var i = startTempoIndex + 1; i < endTempoIndex; i++)
            {
                secs += (TempoList[i + 1].Position - TempoList[i].Position) / (double) TempoList[i].BPM / 8;
            }
            secs += (endTicks - TempoList[endTempoIndex].Position) / (double) TempoList[endTempoIndex].BPM / 8;
            return secs;
        }

        /// <summary>
        /// 计算原始谱面上从某位置（梯）开始经过给定时间（秒）后所对应的实际位置（梯）。
        /// </summary>
        public double GetActualTicksFromSecsOffset(int startTicks, double offsetSecs)
        {
            if (IsAbsoluteTimeMode)
            {
                return GetActualTicksFromTicks(startTicks) + DefaultTempo * 8 * offsetSecs;
            }

            var startTempoIndex = TempoList.FindLastIndex(tempo => tempo.Position <= startTicks);
            double ticks = startTicks;
            var secs = offsetSecs;
            for (var i = startTempoIndex; i < TempoList.Count - 1; i++)
            {
                var dur = (TempoList[i + 1].Position - ticks) / TempoList[i].BPM / 8;
                if (dur < secs)
                {
                    ticks = TempoList[i + 1].Position;
                    secs -= dur;
                }
                else
                {
                    ticks += (TempoList[i + 1].Position - ticks) * secs / dur;
                    return ticks;
                }
            }
            ticks += TempoList.Last().BPM * 8 * secs;
            return ticks;
        }
    }
}
