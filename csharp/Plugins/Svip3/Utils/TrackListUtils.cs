using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using OpenSvip.Model;
using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class TrackListUtils
    {
        public List<Track> Decode(RepeatedField<Any> svip3Tracks)
        {
            var trackList = new List<Track>();
            foreach (var svip3Track in svip3Tracks)
            {
                var type = svip3Track.TypeUrl.Split('.').Last();
                switch (type)
                {
                    case "SingingTrack":
                        var svip3SingingTrack = svip3Track.Unpack<Xstudio.Proto.SingingTrack>();
                        var singingTrack = new SingingTrackUtils().Decode(svip3SingingTrack);
                        trackList.Add(singingTrack);
                        break;
                    case "AudioTrack":
                        var svip3AudioTrack = svip3Track.Unpack<Xstudio.Proto.AudioTrack>();
                        var instrumentalTrack = new AudioTrackUtils().Decode(svip3AudioTrack);
                        trackList.Add(instrumentalTrack);
                        break;
                }
            }
            return trackList;
        }
    }
}
