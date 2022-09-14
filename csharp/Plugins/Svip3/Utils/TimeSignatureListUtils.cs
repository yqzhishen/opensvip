using FlutyDeer.Svip3Plugin.Model;
using OpenSvip.Model;
using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class TimeSignatureListUtils
    {
        public static int FirstBarLength { get; set; }

        public static List<TimeSignature> Decode(List<Xs3TimeSignature> signatures)
        {
            var list = new List<TimeSignature>();
            foreach (var signature in signatures)
            {
                list.Add(new TimeSignature
                {
                    BarIndex = 0,//TODO: 梯到小节数的转换
                    Numerator = signature.Content.Numerator,
                    Denominator = signature.Content.Denominator
                });
            }
            var firstSignature = list.First();
            FirstBarLength = 1920 * firstSignature.Numerator / firstSignature.Denominator;
            return list;
        }

        public static List<Xs3TimeSignature> Encode(List<TimeSignature> signatures)
        {
            var signature = signatures.First();
            var list = new List<Xs3TimeSignature>
            {
                new Xs3TimeSignature
                {
                    Content = new Xs3TimeSignatureContent
                    {
                        Numerator = signature.Numerator,
                        Denominator = signature.Denominator
                    }
                }
            };
            FirstBarLength = 1920 * signature.Numerator / signature.Denominator;
            return list;
        }
    }
}
