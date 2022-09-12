using System;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class SingingTrackUtils
    {
        public OpenSvip.Model.SingingTrack Decode(Xstudio.Proto.SingingTrack track)
        {
            return new OpenSvip.Model.SingingTrack
            {
                Title = track.Name,
                Mute = track.Mute,
                Solo = track.Solo,
                Volume = MathUtils.ToLinearVolume(track.Volume),
                Pan = DecodePan(track.Pan),
                NoteList = new NoteListUtils().Decode(track.PatternList),
                EditedParams = new EditedParamsUtils().Decode(track.PatternList)
            };
        }

        private double DecodePan(double svip3Pan)
        {
            return svip3Pan / 10.0;
        }
    }
}
