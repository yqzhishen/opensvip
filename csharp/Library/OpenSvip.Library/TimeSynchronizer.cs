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
        public List<SongTempo> TempoListAfterOffset { get; }

        private readonly bool IsAbsoluteTimeMode;

        private readonly int DefaultTempo;

        /// <summary>
        /// 实例化一个新的曲谱时间同步器。通常每个工程文件只需要使用一个时间同步器。
        /// </summary>
        /// <param name="originalTempoList">该曲谱的原始曲速列表。</param>
        /// <param name="ignoredTicks">
        ///     指定原始谱面上需要忽略的开头长度，单位为梯。
        ///     例如 OpenSvip Model 中曲速位置的零点比音符位置的零点靠前一个小节，需将第一小节的长度设置为忽略。</param>
        /// <param name="isAbsoluteTimeMode">是否采用绝对时间对齐模式。
        ///     当出现不支持的拍号、曲速等情况时可以开启此模式，将使用恒定曲速进行绝对时间的对齐。</param>
        /// <param name="defaultTempo">当采用绝对时间对齐模式时，可以指定用于对齐的默认曲速。</param>
        public TimeSynchronizer(IList<SongTempo> originalTempoList,
            int ignoredTicks = 0,
            bool isAbsoluteTimeMode = false,
            int defaultTempo = 60)
        {
            TempoListAfterOffset = originalTempoList
                .Where(tempo => tempo.Position >= ignoredTicks)
                .Select(
                    tempo => new SongTempo
                    {
                        Position = tempo.Position - ignoredTicks,
                        BPM = tempo.BPM
                    }).ToList();
            if (!TempoListAfterOffset.Any() || TempoListAfterOffset[0].Position > 0)
            {
                var i = 0;
                for (; i < originalTempoList.Count && originalTempoList[i].Position <= ignoredTicks; i++) { }
                TempoListAfterOffset.Insert(0, new SongTempo
                {
                    Position = 0,
                    BPM = originalTempoList[i - 1].BPM
                });
            }
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
            for (; i < TempoListAfterOffset.Count - 1 && TempoListAfterOffset[i + 1].Position < ticks; i++)
            {
                res += (TempoListAfterOffset[i + 1].Position - TempoListAfterOffset[i].Position) * DefaultTempo / TempoListAfterOffset[i].BPM;
            }
            res += (ticks - TempoListAfterOffset[i].Position) * DefaultTempo / TempoListAfterOffset[i].BPM;
            return res;
        }

        /// <summary>
        /// 将原始谱面位置（梯）转换为对齐后的实际谱面时间坐标（秒）。
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public double GetActualSecsFromTicks(int ticks)
        {
            return GetDurationSecsFromTicks(0, ticks);
        }

        public double GetActualTicksFromSecs(double secs)
        {
            // maybe useless
            return 0;
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
            
            var startTempoIndex = TempoListAfterOffset.FindLastIndex(tempo => tempo.Position <= startTicks);
            var endTempoIndex = TempoListAfterOffset.FindLastIndex(tempo => tempo.Position <= endTicks);
            
            if (startTempoIndex == endTempoIndex)
            {
                return (endTicks - startTicks) / TempoListAfterOffset[startTempoIndex].BPM / 8;
            }
            
            var secs = 0.0;
            secs += (TempoListAfterOffset[startTempoIndex + 1].Position - startTicks)
                    / (double) TempoListAfterOffset[startTempoIndex].BPM / 8;
            for (var i = startTempoIndex + 1; i < endTempoIndex; i++)
            {
                secs += (TempoListAfterOffset[i + 1].Position - TempoListAfterOffset[i].Position) / (double) TempoListAfterOffset[i].BPM / 8;
            }
            secs += (endTicks - TempoListAfterOffset[endTempoIndex].Position) / (double) TempoListAfterOffset[endTempoIndex].BPM / 8;
            return secs;
        }

        public double GetActualTicksFromSecsOffset(int startTicks, double offsetSecs)
        {
            // maybe useless
            return 0;
        }
    }
}