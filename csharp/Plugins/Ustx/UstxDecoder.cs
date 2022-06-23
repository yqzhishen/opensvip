using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenSvip.Model;

using OpenUtau.Core.Ustx;
using OpenUtau.Core;

namespace OxygenDioxide.UstxPlugin
{
    public class UstxDecoder
    {
        public Project DecodeProject(UProject ustxProject)
        {
            //曲速：OpenUTAU每个工程只有一个曲速
            List<SongTempo> songTempoList = new List<SongTempo>();
            songTempoList.Add(new SongTempo {
                Position = 0, 
                BPM = (float)ustxProject.bpm
            });
            //节拍：OpenUTAU每个工程只有一个节拍
            List<TimeSignature> timeSignatureList = new List<TimeSignature>();
            timeSignatureList.Add(new TimeSignature {
                Numerator=ustxProject.beatPerBar,
                Denominator=ustxProject.beatUnit,
            });
            //音轨
            List<Track> trackList = new List<Track>();
            foreach(UTrack ustxTrack in ustxProject.tracks)
            {
                trackList.Add(DecodeTrack(ustxTrack));
            }
            //区段：OpenUTAU的音轨和区段分开存储，因此这里一个个把区段塞进音轨
            foreach(UVoicePart ustxVoicePart in ustxProject.voiceParts)
            {
                DecodeVoicePart(ustxVoicePart,(SingingTrack)trackList[ustxVoicePart.trackNo]);
            }
            Project osProject = new Project
            {
                Version = "SVIP7.0.0",
                SongTempoList = songTempoList,
                TimeSignatureList = timeSignatureList,
                TrackList = trackList
            };
            return osProject;
        }
        public SingingTrack DecodeTrack(UTrack ustxTrack)
        {
            return new SingingTrack
            {
                Title = "",
                Mute = ustxTrack.Mute,
                Solo = ustxTrack.Solo,
                Volume = Math.Min(Math.Pow(10, ustxTrack.Volume / 10), 2.0),//OpenUTAU的音量以分贝存储（#TODO:待实测），这里转为绝对音量
                Pan = 0,
                AISingerName = ustxTrack.singer
            };
        }
        public void DecodeVoicePart(UVoicePart ustxVoicePart, SingingTrack osTrack)
        {
            int partOffset = ustxVoicePart.position;
            foreach(UNote ustxNote in ustxVoicePart.notes)
            {
                osTrack.NoteList.Add(DecodeNote(ustxNote,partOffset));
            }
        }
        public Note DecodeNote(UNote ustxNote,int partOffset)
        {
            string lyric = ustxNote.lyric;
            //OpenUTAU的连音符为+，多音节词可能还有+~、+4等形式，这里统一转为-
            if (lyric.StartsWith("+"))
            {
                lyric = "-";
            }
            return new Note
            {
                StartPos = ustxNote.position + partOffset,
                Length = ustxNote.duration,
                KeyNumber = ustxNote.tone,
                Lyric = lyric
            };
        }
    }
}