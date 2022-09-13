using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using OpenSvip.Model;
using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class TrackListUtils
    {
        #region Decoding

        public List<Track> Decode(RepeatedField<Any> svip3Tracks)
        {
            var tracks = new List<Track>();
            foreach (var svip3Track in svip3Tracks)
            {
                var type = svip3Track.TypeUrl.Split('.').Last();
                switch (type)
                {
                    case "SingingTrack":
                        var svip3SingingTrack = svip3Track.Unpack<Xstudio.Proto.SingingTrack>();
                        var singingTrack = new SingingTrackUtils().Decode(svip3SingingTrack);
                        tracks.Add(singingTrack);
                        break;
                    case "AudioTrack":
                        var svip3AudioTrack = svip3Track.Unpack<Xstudio.Proto.AudioTrack>();
                        var instrumentalTrack = new AudioTrackUtils().Decode(svip3AudioTrack);
                        tracks.Add(instrumentalTrack);
                        break;
                }
            }
            return tracks;
        }

        #endregion

        #region Encoding

        public RepeatedField<Any> Encode(List<Track> tracks)
        {
            var field = new RepeatedField<Any>();
            foreach (var track in tracks)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        var svip3SingingTrack = new SingingTrackUtils().Encode(singingTrack);
                        var singingTrackmessage = Any.Pack(svip3SingingTrack);
                        field.Add(singingTrackmessage);
                        break;
                    case InstrumentalTrack instrumentalTrack:
                        var svip3AudioTrack = new AudioTrackUtils().Encode(instrumentalTrack);
                        var audioTrackMessage = Any.Pack(svip3AudioTrack);
                        field.Add(audioTrackMessage);
                        break;
                }
            }
            return field;
        }

        #endregion
    }
}
