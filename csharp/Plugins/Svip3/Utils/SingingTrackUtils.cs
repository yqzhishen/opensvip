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
                Volume = DecodeVolume(track.Volume),
                Pan = DecodePan(track.Pan),
                NoteList = new NoteListUtils().Decode(track.PatternList)
            };
        }

        private double DecodeVolume(double gain)
        {
            return gain >= 0
                ? Math.Min(gain / (20 * Math.Log10(4)) + 1.0, 2.0)
                : Math.Pow(10, gain / 20.0);
        }

        private double DecodePan(double svip3Pan)
        {
            return svip3Pan / 10.0;
        }
    }
}
