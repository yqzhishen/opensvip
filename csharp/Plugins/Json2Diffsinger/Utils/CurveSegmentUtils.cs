using System;
using System.Collections.Generic;
using System.Linq;

namespace Json2DiffSinger.Utils
{
    public static class CurveSegmentUtils
    {
        public static double GetValueFromSegment(List<Tuple<int, int>> segment, double ticks)
        {
            var leftPoint = segment.BinaryFindLast(p => p.Item1 <= ticks);
            if (leftPoint == null)
            {
                return segment.First().Item2;
            }

            var rightPoint = segment.BinaryFindFirst(p => p.Item1 > ticks);
            if (rightPoint == null)
            {
                return segment.Last().Item2;
            }

            var ratio = (ticks - leftPoint.Item1) / (rightPoint.Item1 - leftPoint.Item1);
            return (1 - ratio) * leftPoint.Item2 + ratio * rightPoint.Item2;
        }
    }
}