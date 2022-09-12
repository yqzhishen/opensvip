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
    public static class CommonParamUtils
    {
        public static ParamCurve Decode(RepeatedField<SingingPattern> patterns)
        {
            return new ParamCurve();
        }
    }
}
