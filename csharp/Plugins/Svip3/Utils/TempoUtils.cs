using FlutyDeer.Svip3Plugin.Model;
using OpenSvip.Library;
using OpenSvip.Model;
using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class TempoUtils
    {
        public static List<SongTempo> SongTempoList { get; set; }

        public static TimeSynchronizer Synchronizer { get; set; }

        public static List<SongTempo> Decode(List<Xs3Tempo> tempos)
        {
            var list = new List<SongTempo>();
            foreach (var tempo in tempos)
            {
                list.Add(new SongTempo
                {
                    Position =0,
                    BPM = tempo.Tempo
                });
            }
            SongTempoList = list;
            return list;
        }

        public static List<Xs3Tempo> Encode(List<SongTempo> tempos)
        {
            var firstTempo = tempos.First();
            var list = new List<Xs3Tempo>
            {
                new Xs3Tempo
                {
                    Tempo = firstTempo.BPM
                }
            };
            Synchronizer = new TimeSynchronizer(tempos);
            return list;
        }
    }
}
