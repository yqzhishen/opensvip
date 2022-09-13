using Google.Protobuf.Collections;
using OpenSvip.Library;
using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class TempoUtils
    {
        public static List<OpenSvip.Model.SongTempo> SongTempoList { get; set; }

        public static TimeSynchronizer Synchronizer { get; set; }

        public static List<OpenSvip.Model.SongTempo> Decode(RepeatedField<Xstudio.Proto.SongTempo> tempos)
        {
            var list = new List<OpenSvip.Model.SongTempo>();
            foreach (var tempo in tempos)
            {
                list.Add(new OpenSvip.Model.SongTempo
                {
                    Position = tempo.Pos,
                    BPM = tempo.Tempo / 100.0f
                });
            }
            SongTempoList = list;
            return list;
        }

        public static RepeatedField<Xstudio.Proto.SongTempo> Encode(List<OpenSvip.Model.SongTempo> tempos)
        {
            var firstTempo = tempos.First();
            var field = new RepeatedField<Xstudio.Proto.SongTempo>
            {
                new Xstudio.Proto.SongTempo
                {
                    Pos = 0,
                    Tempo = (int)(firstTempo.BPM * 100)
                }
            };
            Synchronizer = new TimeSynchronizer(tempos);
            return field;
        }
    }
}
