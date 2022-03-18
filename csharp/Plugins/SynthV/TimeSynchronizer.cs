using System.Collections.Generic;
using OpenSvip.Model;

namespace Plugin.SynthV
{
    public class TimeSynchronizer
    {
        private readonly List<SongTempo> TempoBuffer;

        private readonly bool IsAbsoluteTimeMode;

        private readonly int DefaultTempo;

        public TimeSynchronizer(List<SongTempo> originalTempoList, bool isAbsoluteTimeMode, int defaultTempo)
        {
            TempoBuffer = originalTempoList;
            IsAbsoluteTimeMode = isAbsoluteTimeMode;
            DefaultTempo = defaultTempo;
        }

        public double GetActualTicksFromTicks(int ticks)
        {
            if (!IsAbsoluteTimeMode)
            {
                return ticks;
            }
            var res = 0.0;
            var i = 0;
            for (; i < TempoBuffer.Count - 1 && TempoBuffer[i + 1].Position < ticks; i++)
            {
                res += (TempoBuffer[i + 1].Position - TempoBuffer[i].Position) * DefaultTempo / TempoBuffer[i].BPM;
            }
            res += (ticks - TempoBuffer[i].Position) * DefaultTempo / TempoBuffer[i].BPM;
            return res;
        }

        public double GetActualSecsFromTicks(int ticks)
        {
            return GetDurationSecsFromTicks(0, ticks);
        }

        public double GetActualTicksFromSecs(double secs)
        {
            // maybe useless
            return 0;
        }

        public double GetDurationSecsFromTicks(int startTicks, int endTicks)
        {
            if (IsAbsoluteTimeMode)
            {
                return (GetActualTicksFromTicks(endTicks) - GetActualTicksFromTicks(startTicks)) / DefaultTempo / 8;
            }
            
            var startTempoIndex = TempoBuffer.FindLastIndex(tempo => tempo.Position <= startTicks);
            var endTempoIndex = TempoBuffer.FindIndex(tempo => tempo.Position >= endTicks);
            
            if (endTempoIndex == -1 || startTempoIndex + 1 == endTempoIndex)
            {
                return (endTicks - startTicks) / TempoBuffer[startTempoIndex].BPM / 8;
            }
            
            var secs = 0.0;
            secs += (TempoBuffer[startTempoIndex + 1].Position - startTicks)
                    / (double) TempoBuffer[startTempoIndex].BPM / 8;
            for (var i = startTempoIndex + 1; i < endTempoIndex - 1; i++)
            {
                secs += (TempoBuffer[i + 1].Position - TempoBuffer[i].Position) / (double) TempoBuffer[i].BPM / 8;
            }
            secs += (endTicks - TempoBuffer[endTempoIndex - 1].Position) / (double) TempoBuffer[endTempoIndex - 1].BPM / 8;
            return secs;
        }

        public double GetActualTicksFromSecsOffset(int startTicks, double offsetSecs)
        {
            return 0;
        }
    }
}