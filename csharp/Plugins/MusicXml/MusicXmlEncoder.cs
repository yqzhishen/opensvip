using FlutyDeer.MusicXml.Core;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlutyDeer.MusicXml
{
    public class MusicXmlEncoder
    {
        public ScorePartWise EncodeMusicXml(Project project)
        {
            var scorePartWise =  new ScorePartWise
            {
                Version = new decimal(3.1)
            };
            return scorePartWise;
        }
    }
}
