using Google.Protobuf.Collections;
using OpenSvip.Model;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class EditedParamsUtils
    {
        public Params Decode(RepeatedField<SingingPattern> patterns)
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
