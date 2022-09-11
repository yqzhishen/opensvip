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
    public class EditedParamsUtils
    {
        public Params Decode(RepeatedField<SingingPattern> patterns)
        {
            var @params = new Params();
            return @params;
        }
    }
}
