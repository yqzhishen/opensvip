using Melanchall.DryWetMidi.Interaction;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    BPM = 60000000 / tempo.MicrosecondsPerQuarterNote
                });
            }
            return songTempoList;
        }
    }
}
