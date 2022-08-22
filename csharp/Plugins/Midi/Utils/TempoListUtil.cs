using Melanchall.DryWetMidi.Interaction;
using OpenSvip.Model;
using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.MidiPlugin.Utils
{
    public static class TempoListUtil
    {
        public static List<SongTempo> DecodeSongTempoList(TempoMap tempoMap, short PPQ)
        {
            List<SongTempo> songTempoList = new List<SongTempo>();
            var changes = tempoMap.GetTempoChanges();
            if (changes != null && changes.Count() > 0)
            {
                var firstTempoTime = changes.First().Time;
                if (firstTempoTime > 0)//首个曲速标记不在0，添加120BPM
                {
                    songTempoList.Add(new SongTempo
                    {
                        Position = 0,
                        BPM = 120.0f
                    });
                }
                foreach (var change in changes)
                {
                    var tempo = change.Value;
                    var time = change.Time * 480 / PPQ;
                    songTempoList.Add(new SongTempo
                    {
                        Position = (int)time,
                        BPM = (float)(60.0 / tempo.MicrosecondsPerQuarterNote * 1000000.0)
                    });
                }
            }
            else//曲速列表为空
            {
                songTempoList.Add(new SongTempo
                {
                    Position = 0,
                    BPM = 120.0f
                });
            }
            return songTempoList;
        }
    }
}
