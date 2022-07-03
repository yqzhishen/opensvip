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
                beatUnit = osProject.TimeSignatureList[0].Denominator,
                voiceParts = new List<UVoicePart>(),
                ustxVersion = new System.Version("0.5")
                
            };
            int trackNo = 0;
            foreach (Track osTrack in osProject.TrackList)
            {
                ustxProject.tracks.Add(EncodeTrack(osTrack));
                if(osTrack.Type=="Singing")//合成音轨
                {
                    ustxProject.voiceParts.Add(EncodeVoicePart((SingingTrack)osTrack,trackNo));
                }
                else
                {
                    ustxProject.waveParts.Add(EncodeWavePart((InstrumentalTrack)osTrack,trackNo));
                }
                trackNo += 1;
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
        public UVoicePart EncodeVoicePart(SingingTrack osTrack, int trackNo)
        {
            UVoicePart ustxVoicePart = new UVoicePart {
                name = osTrack.Title,
                trackNo = trackNo,
                position = 0
            };
            int lastNoteEndPos = -480;//上一个音符的结束时间
            int lastNoteKeyNumber = 60;//上一个音符的音高
            foreach (Note osNote in osTrack.NoteList)
            {
                ustxVoicePart.notes.Add(EncodeNote(osNote,lastNoteEndPos>=osNote.StartPos,lastNoteKeyNumber));
                lastNoteEndPos = osNote.StartPos + osNote.Length;
                lastNoteKeyNumber = osNote.KeyNumber;
            }
            return ustxVoicePart;
        }
        public UWavePart EncodeWavePart(InstrumentalTrack osTrack, int trackNo)
        {
            UWavePart ustxWavePart = new UWavePart
            {
                name = osTrack.Title,
                trackNo = trackNo,
                position = 0,
                FilePath = osTrack.AudioFilePath
            };
            return ustxWavePart;
        }
        public UNote EncodeNote(Note osNote,bool snapFirst,int lastNoteKeyNumber)
        {
            //snapFirst：是否与上一个音符挨着，挨着就是True
            //lastNoteKeyNumber：上一个音符的音高
            int Y0 = 0;
            if(snapFirst==true)
            {
                Y0=(lastNoteKeyNumber-osNote.KeyNumber)*10;
            }
            string lyric = osNote.Lyric;
            if ("-" ==  lyric)
            {
                lyric = "+";
            }
            UNote ustxNote = new UNote{
                position = osNote.StartPos,
                duration = osNote.Length,
                tone = osNote.KeyNumber,
                lyric = lyric,
                pitch = new UPitch
                {
                    snapFirst=snapFirst,
                    data= new List<PitchPoint>
                    {
                        new PitchPoint {X=-40,Y=0,shape=PitchPointShape.io},
                        new PitchPoint {X=0,Y=0,shape=PitchPointShape.io}
                    }
                },
                vibrato = new UVibrato
                {
                    length = 0,
                    period = 175,
                    depth = 25,
                    @in = 10,
                    @out = 10,
                    shift = 0,
                    drift = 0
                }
            };
            return ustxNote;
        }
    }
}