using FlutyDeer.Svip3Plugin.Utils;
using OpenSvip.Model;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin
{
    public class Svip3Decoder
    {
        public Project Decode(AppModel model)
        {
            return new Project
            {
                TimeSignatureList = TimeSignatureListUtils.Decode(model.BeatList),
                SongTempoList = TempoUtils.Decode(model.TempoList),
                TrackList = new TrackListUtils().Decode(model.TrackList)
            };
        }
    }
}
