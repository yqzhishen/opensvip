using System;
using System.Linq;

namespace AceStdio.Options
{
    public class NormalizationArgs
    {
        public NormalizationMethod Method = NormalizationMethod.None;

        public double LowerThreshold;

        public double UpperThreshold;

        public double Scale;

        public double Bias;

        public static bool TryParse(string s, out NormalizationArgs result)
        {
            result = new NormalizationArgs();
            var args = s.Split(',', '，').Select(arg => arg.Trim()).ToArray();
            
            if (args.Length < 5)
            {
                goto fail;
            }
            
            if (!Enum.TryParse(args[0], true, out result.Method))
            {
                goto fail;
            }

            if (!double.TryParse(args[1], out result.LowerThreshold))
            {
                goto fail;
            }

            if (!double.TryParse(args[2], out result.UpperThreshold))
            {
                goto fail;
            }

            if (!double.TryParse(args[3], out result.Scale))
            {
                goto fail;
            }

            if (!double.TryParse(args[4], out result.Bias))
            {
                goto fail;
            }

            result.LowerThreshold = Math.Max(0, Math.Min(10.0, result.LowerThreshold)) / 10.0;
            result.UpperThreshold = Math.Max(0, Math.Min(10.0, result.UpperThreshold)) / 10.0;
            result.Scale = Math.Max(-1.0, Math.Min(1.0, result.Scale));
            result.Bias = Math.Max(-1.0, Math.Min(1.0, result.Bias));
            return true;
            
            fail:
            result = new NormalizationArgs();
            return false;
        }

        public bool IsEnabled => Method != NormalizationMethod.None;
    }

    public enum NormalizationMethod
    {
        None, ZScore, MinMax
    }
}