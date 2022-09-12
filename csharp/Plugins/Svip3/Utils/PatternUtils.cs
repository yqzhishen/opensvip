using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class PatternUtils
    {
        public static Tuple<int, int> GetVisiableRange(SingingPattern pattern)
        {
            int left = pattern.PlayPos + pattern.RealPos;
            int right = left + pattern.PlayDur;
            return new Tuple<int, int>(left, right);
        }
    }
}
