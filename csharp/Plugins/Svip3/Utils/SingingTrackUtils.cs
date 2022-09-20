using FlutyDeer.Svip3Plugin.Model;
using OpenSvip.Model;
using System.Linq;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class SingingTrackUtils
    {
        #region Decoding
        public SingingTrack Decode(Xs3SingingTrack track)
        {
            return new SingingTrack
            {
                Title = track.Name,
                Mute = track.Mute,
                Solo = track.Solo,
                Volume = MathUtils.ToLinearVolume(track.Gain),
                Pan = DecodePan(track.Pan),
                NoteList = new NoteListUtils().Decode(track.PatternList),
                EditedParams = new ParamsUtils().Decode(track.PatternList),
                AISingerName = Singers.GetName(track.SingerId)
            };
        }

        private double DecodePan(double pan)
        {
            return pan / 10.0;
        }

        #endregion

        #region Encoding

        public Xs3SingingTrack Encode(SingingTrack track, string color)
        {
            var singingTrack = new Xs3SingingTrack
            {
                Name = track.Title,
                Mute = track.Mute,
                Solo = track.Solo,
                Gain = MathUtils.ToDecibelVolume(track.Volume),
                Pan = EncodePan(track.Pan),
                SingerId = Singers.GetUuid(track.AISingerName),
                Color = color
            };
            if (track != null && track.NoteList != null && track.NoteList.Any())
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
