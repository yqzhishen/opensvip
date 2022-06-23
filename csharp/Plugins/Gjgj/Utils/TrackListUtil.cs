using FlutyDeer.GjgjPlugin.Model;
using OpenSvip.Framework;
using OpenSvip.Model;
using System.Collections.Generic;

namespace FlutyDeer.GjgjPlugin.Utils
{
    public static class TrackListUtil
    {

        /// <summary>
        /// 转换演唱轨和伴奏轨。
        /// </summary>
        public static void EncodeTracks(GjProject gjProject, Project osProject)
        {
            int noteID = 1;
            int trackID = 1;
            gjProject.SingingTrackList = new List<GjSingingTrack>(osProject.TrackList.Count);
            gjProject.InstrumentalTrackList = new List<GjInstrumentalTrack>();
            int trackIndex = 0;
            foreach (var track in osProject.TrackList)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        gjProject.SingingTrackList.Add(TrackUtil.EncodeSingingTrack(ref noteID, trackID, singingTrack));
                        trackID++;
                        break;
                    case InstrumentalTrack instrumentalTrack:
                        gjProject.InstrumentalTrackList.Add(TrackUtil.EncodeInstrumentalTrack(trackID, instrumentalTrack));
                        trackID++;
                        break;
                    default:
                        break;
                }
                trackIndex++;
            }
            if (UnsupportedPinyin.UnsupportedPinyins.Count > 0)
            {
                string unsupportedPinyin = string.Join("、", UnsupportedPinyin.UnsupportedPinyins);
                Warnings.AddWarning($"当前工程文件有歌叽歌叽不支持的拼音，已忽略。不支持的拼音：{unsupportedPinyin}", type: WarningTypes.Lyrics);
            }
            gjProject.MIDITrackList = new List<GjMIDITrack>();
        }
    }
}