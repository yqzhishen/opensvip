using FlutyDeer.Svip3Plugin.Utils;
using OpenSvip.Model;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin
{
    public class Svip3Encoder
    {
        public AppModel Encode(Project project)
        {
            var model = new AppModel();
            model.BeatList.AddRange(TimeSignatureListUtils.Encode(project.TimeSignatureList));
            model.TempoList.AddRange(TempoUtils.Encode(project.SongTempoList));
            model.TrackList.AddRange(new TrackListUtils().Encode(project.TrackList));
            return model;
        }
    }
}
