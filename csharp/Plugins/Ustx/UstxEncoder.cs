using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenSvip.Model;

using OpenUtau.Core.Ustx;
using OpenUtau.Core;

namespace OxygenDioxide.UstxPlugin.Stream
{
    public class UstxEncoder
    {
        public UProject EncodeProject(Project osProject)
        {
            UProject ustxProject = new UProject {
                bpm = osProject.SongTempoList[0].BPM,//OpenUTAU不支持变速，因此这里采用音轨开头的bpm
                beatPerBar = osProject.TimeSignatureList[0].Numerator,
                beatUnit = osProject.TimeSignatureList[0].Denominator
            };
            int trackNo = 0;
            foreach (Track osTrack in osProject.TrackList)
            {
                ustxProject.tracks.Add(EncodeTrack(osTrack));
                trackNo += 1;
                if(osTrack.Type=="Singing")//合成音轨
                {
                    ustxProject.voiceParts.Add(EncodeVoicePart((SingingTrack)osTrack));
                }
                else
                {
                    //#TODO：伴奏音轨
                }
            }
            return ustxProject;
        }
        public UTrack EncodeTrack(Track osTrack)
        {
            UTrack ustxTrack = new UTrack {
                singer = "",
                phonemizer = "OpenUtau.Core.DefaultPhonemizer",//默认音素器
                renderer = "CLASSIC",//经典渲染器
                Mute = osTrack.Mute,
                Solo = osTrack.Solo,
                Volume = Math.Log10(osTrack.Volume)*10//绝对音量转对数音量
            };
            return ustxTrack;
        }
        public UVoicePart EncodeVoicePart(SingingTrack osTrack)
        {
            UVoicePart ustxVoicePart = new UVoicePart { 
                
            };
            return ustxVoicePart;
        }
        public UNote EncodeNote(Note osNote)
        {
            UNote ustxNote = new UNote{
                position = osNote.StartPos,
                duration = osNote.Length,
                tone = osNote.KeyNumber,
                lyric = osNote.Lyric
            };
            return ustxNote;
        }
    }
}