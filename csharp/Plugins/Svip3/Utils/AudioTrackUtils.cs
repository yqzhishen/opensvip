using FlutyDeer.Svip3Plugin.Model;
using OpenSvip.Model;
using System.Linq;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class AudioTrackUtils
    {
        #region Decoding
        public InstrumentalTrack Decode(Xs3AudioTrack track)
        {
            string audioFilePath = null;
            int offset = 0;
            if (track.PatternList.Any())
            {
                var firstPattern = track.PatternList[0];
                audioFilePath = firstPattern.AudioFilePath;
                offset = firstPattern.OriginalStartPosition;
            }
            return new InstrumentalTrack
            {
                Title = track.Name,
                Mute = track.Mute,
                Solo = track.Solo,
                Volume = MathUtils.ToLinearVolume(track.Gain),
                Pan = DecodePan(track.Pan),
                AudioFilePath = audioFilePath,
                Offset = offset
            };
        }

        private double DecodePan(double svip3Pan)
        {
            return svip3Pan / 10.0;
        }

        #endregion

        public Xs3AudioTrack Encode(InstrumentalTrack track)
        {
            var Xs3AudioTrack = new Xs3AudioTrack
            {
                Name = track.Title,
                Mute = track.Mute,
                Solo = track.Solo,
                Gain = MathUtils.ToDecibelVolume(track.Volume),
                Pan = EncodePan(track.Pan)
            };
            Xs3AudioTrack.PatternList.AddRange(PatternUtils.Encode(track));
            return Xs3AudioTrack;
        }

        private float EncodePan(double pan)
        {
            return (float)(pan * 10.0);
        }
    }
}
