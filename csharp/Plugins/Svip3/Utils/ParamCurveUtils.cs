using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
