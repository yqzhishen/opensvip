using FlutyDeer.Svip3Plugin.Model;
using FlutyDeer.Svip3Plugin.Utils;
using OpenSvip.Model;

namespace FlutyDeer.Svip3Plugin
{
    public class Svip3Encoder
    {
        public Svip3Model Encode(Project project)
        {
            return new Svip3Model
            {
                TimeSignatureList = TimeSignatureListUtils.Encode(project.TimeSignatureList),
                TempoList = TempoUtils.Encode(project.SongTempoList),
                TrackList = TrackListUtils.Encode(project.TrackList),
                Duration = TrackListUtils.SongDuration
            };
        }
    }
}
