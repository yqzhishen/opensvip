using Melanchall.DryWetMidi.Interaction;
using OpenSvip.Model;
using System.Collections.Generic;

namespace FlutyDeer.MidiPlugin.Utils
{
    public static class TempoListUtil
    {
        public static List<SongTempo> DecodeSongTempoList(TempoMap tempoMap)
        {
            List<SongTempo> songTempoList = new List<SongTempo>();
            var changes = tempoMap.GetTempoChanges();
            foreach (var change in changes)
            {
                var tempo = change.Value;
                var time = change.Time;
                songTempoList.Add(new SongTempo
                {
                    Position = (int)time,
                    BPM = (float)(60.0 / tempo.MicrosecondsPerQuarterNote * 1000000.0)
                });
            }
            return songTempoList;
        }
    }
}
