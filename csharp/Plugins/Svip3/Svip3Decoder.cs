using FlutyDeer.Svip3Plugin.Model;
using FlutyDeer.Svip3Plugin.Utils;
using OpenSvip.Model;

namespace FlutyDeer.Svip3Plugin
{
    public class Svip3Decoder
    {
        public Project Decode(Svip3Model model)
        {
            return new Project
            {
                TimeSignatureList = TimeSignatureListUtils.Decode(model.TimeSignatureList),
                SongTempoList = TempoUtils.Decode(model.TempoList),
                TrackList = TrackListUtils.Decode(model.TrackList)
            };
        }
    }
}
