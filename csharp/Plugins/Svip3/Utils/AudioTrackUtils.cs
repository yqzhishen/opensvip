using OpenSvip.Model;
using System.Linq;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class AudioTrackUtils
    {
        #region Decoding
        public InstrumentalTrack Decode(AudioTrack track)
        {
            string audioFilePath = null;
            int offset = 0;
            if (track.PatternList.Any())
            {
                var firstPattern = track.PatternList[0];
                audioFilePath = firstPattern.AudioFilePath;
                offset = firstPattern.RealPos;
            }
            return new InstrumentalTrack
            {
                Title = track.Name,
                Mute = track.Mute,
                Solo = track.Solo,
                Volume = MathUtils.ToLinearVolume(track.Volume),
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

        public AudioTrack Encode(InstrumentalTrack track)
        {
            var audioTrack = new AudioTrack
            {
                Name = track.Title,
                Mute = track.Mute,
                Solo = track.Solo,
                Volume = MathUtils.ToDecibelVolume(track.Volume),
                Pan = EncodePan(track.Pan)
            };
            audioTrack.PatternList.AddRange(PatternUtils.Encode(track));
            return audioTrack;
        }

        private float EncodePan(double pan)
        {
            return (float)(pan * 10.0);
        }
    }
}
