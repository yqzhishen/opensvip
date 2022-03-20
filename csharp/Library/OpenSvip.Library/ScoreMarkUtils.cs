using System.Collections.Generic;
using System.Linq;
using OpenSvip.Model;

namespace OpenSvip.Library
{
    public static class ScoreMarkUtils
    {
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
    }
}