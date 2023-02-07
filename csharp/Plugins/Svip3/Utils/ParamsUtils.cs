using FlutyDeer.Svip3Plugin.Model;
using OpenSvip.Model;
using System.Collections.Generic;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class ParamsUtils
    {
        public Params Decode(List<Xs3SingingPattern> patterns)
        {
            var @params = new Params
            {
                Pitch = PitchParamUtils.Decode(patterns),
                Volume = CommonParamUtils.Decode(patterns),
                Strength = CommonParamUtils.Decode(patterns)
            };
            return @params;
        }

    }
}
