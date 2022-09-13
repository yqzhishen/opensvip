using OpenSvip.Model;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class SingingTrackUtils
    {
        #region Decoding
        public SingingTrack Decode(Xstudio.Proto.SingingTrack track)
        {
            return new SingingTrack
            {
                Title = track.Name,
                Mute = track.Mute,
                Solo = track.Solo,
                Volume = MathUtils.ToLinearVolume(track.Volume),
                Pan = DecodePan(track.Pan),
                NoteList = new NoteListUtils().Decode(track.PatternList),
                EditedParams = new EditedParamsUtils().Decode(track.PatternList),
                AISingerName = Singers.GetName(track.AiSingerId)
            };
        }

        private double DecodePan(double pan)
        {
            return pan / 10.0;
        }

        #endregion

        #region Encoding

        public Xstudio.Proto.SingingTrack Encode(SingingTrack track)
        {
            var singingTrack = new Xstudio.Proto.SingingTrack
            {
                Name = track.Title,
                Mute = track.Mute,
                Solo = track.Solo,
                Volume = MathUtils.ToDecibelVolume(track.Volume),
                Pan = EncodePan(track.Pan),
                Color = "#66CCFF"
            };
            singingTrack.PatternList.AddRange(PatternUtils.Encode(track));
            return singingTrack;
        }

        private float EncodePan(double pan)
        {
            return (float)(pan * 10.0);
        }

        #endregion
    }
}
