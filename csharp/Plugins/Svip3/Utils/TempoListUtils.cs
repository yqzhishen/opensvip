using Google.Protobuf.Collections;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class TempoListUtils
    {
        public static List<OpenSvip.Model.SongTempo> SongTempoList { get; set; }

        public static List<OpenSvip.Model.SongTempo> Decode(RepeatedField<Xstudio.Proto.SongTempo> tempos)
        {
            var songTempoList = new List<OpenSvip.Model.SongTempo>();
            foreach (var tempo in tempos)
            {
                songTempoList.Add(new OpenSvip.Model.SongTempo
                {
                    Position = tempo.Pos,
                    BPM = tempo.Tempo / 100.0f
                });
            }
            SongTempoList = songTempoList;
            return songTempoList;
        }
    }
}
