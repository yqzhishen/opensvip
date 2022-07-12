using System.Collections.Generic;
using AceStdio.Model;

namespace AceStdio.Utils
{
    public static class TimeUtils
    {
        public static double TickToSecond(int tick, List<AceTempo> tempoList)
        {
            var tempoIndex = tempoList.FindLastIndex(tempo => tempo.Position <= tick);
            
            if (tempoIndex == 0)
            {
                return tick / tempoList[0].BPM / 8;
            }
            
            var secs = 0.0;
            secs += tempoList[1].Position / tempoList[0].BPM / 8;
            for (var i = 1; i < tempoIndex; i++)
            {
                secs += (tempoList[i + 1].Position - tempoList[i].Position) / tempoList[i].BPM / 8;
            }
            secs += (tick - tempoList[tempoIndex].Position) / tempoList[tempoIndex].BPM / 8;
            return secs;
        }
    }
}