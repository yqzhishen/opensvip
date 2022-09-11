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
            var timeSignatureList = new List<TimeSignature>();
            foreach (var beat in beats)
            {
                timeSignatureList.Add(new TimeSignature
                {
                    BarIndex = 0,//TODO: 梯到小节数的转换
                    Numerator = beat.BeatSize.Numerator,
                    Denominator = beat.BeatSize.Denominator
                });
            }
            var fistTimeSignature = timeSignatureList.First();
            FirstBarLength = 1920 * fistTimeSignature.Numerator / fistTimeSignature.Denominator;
            return timeSignatureList;
        }
    }
}
