using OpenSvip.Model;
using System.Collections.Generic;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class ParamCurveUtils
    {
        public static ParamCurve Merge(List<ParamCurve> curves)
        {
            var mergedCurve = new ParamCurve();
            foreach (var curve in curves)
            {
                var points = curve.PointList;
                mergedCurve.PointList.AddRange(points);
            }
            return mergedCurve;
        }
    }
}
