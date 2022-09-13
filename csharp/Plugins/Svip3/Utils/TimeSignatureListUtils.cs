using Google.Protobuf.Collections;
using OpenSvip.Model;
using System.Collections.Generic;
using System.Linq;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class TimeSignatureListUtils
    {
        public static int FirstBarLength { get; set; }

        public static List<TimeSignature> Decode(RepeatedField<SongBeat> beats)
        {
            var list = new List<TimeSignature>();
            foreach (var beat in beats)
            {
                list.Add(new TimeSignature
                {
                    BarIndex = 0,//TODO: 梯到小节数的转换
                    Numerator = beat.BeatSize.Numerator,
                    Denominator = beat.BeatSize.Denominator
                });
            }
            var signature = list.First();
            FirstBarLength = 1920 * signature.Numerator / signature.Denominator;
            return list;
        }

        public static RepeatedField<SongBeat> Encode(List<TimeSignature> signatures)
        {
            var signature = signatures.First();
            var field = new RepeatedField<SongBeat>
            {
                new SongBeat
                {
                    Pos = 0,
                    BeatSize = new BeatSize
                    {
                        Numerator = signature.Numerator,
                        Denominator = signature.Denominator
                    }
                }
            };
            FirstBarLength = 1920 * signature.Numerator / signature.Denominator;
            return field;
        }
    }
}
