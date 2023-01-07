using FlutyDeer.Svip3Plugin.Model;
using OpenSvip.Model;
using System;
using System.Collections.Generic;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class TrackListUtils
    {
        public static int SongDuration { get; set; }

        #region Decoding

        public static List<Track> Decode(List<object> svip3Tracks)
        {
            var tracks = new List<Track>();
            foreach (var svip3Track in svip3Tracks)
            {
                switch (svip3Track)
                {
                    case Xs3SingingTrack xs3SingingTrack:
                        var singingTrack = new SingingTrackUtils().Decode(xs3SingingTrack);
                        tracks.Add(singingTrack);
                        break;
                    case Xs3AudioTrack xs3AudioTrack:
                        var instrumentalTrack = new AudioTrackUtils().Decode(xs3AudioTrack);
                        tracks.Add(instrumentalTrack);
                        break;
                }
            }
            return tracks;
        }

        #endregion

        #region Encoding

        public static List<object> Encode(List<Track> tracks)
        {
            var list = new List<object>();
            int colorIndex = new Random().Next(Constants.Colors.Count);
            string color = Constants.Colors.GetColor(colorIndex);
            foreach (var track in tracks)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        var svip3SingingTrack = new SingingTrackUtils().Encode(singingTrack, color);
                        list.Add(svip3SingingTrack);
                        break;
                    case InstrumentalTrack instrumentalTrack:
                        var svip3AudioTrack = new AudioTrackUtils().Encode(instrumentalTrack, color);
                        list.Add(svip3AudioTrack);
                        break;
                }
                colorIndex++;
            }
            return list;
        }

        #endregion
    }
}
