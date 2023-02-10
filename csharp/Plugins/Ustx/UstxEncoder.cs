using System;
using System.Collections.Generic;
using System.Linq;
using OpenSvip.Model;
using OpenUtau.Core.Ustx;

namespace OxygenDioxide.UstxPlugin.Stream
{
    public static class UstxEncoder
    {
        public static UProject EncodeProject(Project osProject)
        {
            //曲速
            List<UTempo> tempos = osProject.SongTempoList
                .Select(EncodeTempo)
                .ToList();
            if(tempos.Count==0)
            {
                tempos.Add(new UTempo
                {
                    bpm = 120,
                    position = 0,
                });
            }

            //节拍
            List<UTimeSignature> ustxTimeSignatures = new List<UTimeSignature> { 
                new UTimeSignature(0,4,4)
            };
            
            UProject ustxProject = new UProject {
                tempos = tempos,
                bpm = tempos[0].bpm,
                timeSignatures = ustxTimeSignatures,
                voiceParts = new List<UVoicePart>(),
                waveParts = new List<UWavePart>(),
                ustxVersion = new System.Version("0.6")
            };
            
            int trackNo = 0;
            ustxProject.tracks = new List<UTrack>();
            foreach (Track osTrack in osProject.TrackList)
            {
                ustxProject.tracks.Add(EncodeTrack(osTrack));
                if(osTrack.Type=="Singing")//合成音轨
                {
                    ustxProject.voiceParts.Add(EncodeVoicePart((SingingTrack)osTrack,trackNo,ustxProject));
                }
                else//伴奏音轨
                {
                    ustxProject.waveParts.Add(EncodeWavePart((InstrumentalTrack)osTrack,trackNo));
                }
                trackNo += 1;
            }
            return ustxProject;
        }

        public static UTempo EncodeTempo(SongTempo osTempo)
        {
            return new UTempo
            {
                position = Math.Max(osTempo.Position - 1920, 0),
                bpm = osTempo.BPM
            };
        }

        public static UTrack EncodeTrack(Track osTrack)
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
        public static UVoicePart EncodeVoicePart(SingingTrack osTrack, int trackNo, UProject ustxProject)
        {
            UVoicePart ustxVoicePart = new UVoicePart {
                name = osTrack.Title,
                trackNo = trackNo,
                position = 0
            };
            if (osTrack.NoteList.Count == 0)
            {
                return ustxVoicePart;
            }
            int lastNoteEndPos = -480;//上一个音符的结束时间
            int lastNoteKeyNumber = 60;//上一个音符的音高
            //转换音符
            foreach (Note osNote in osTrack.NoteList)
            {
                ustxVoicePart.notes.Add(EncodeNote(osNote,lastNoteEndPos>=osNote.StartPos,lastNoteKeyNumber));
                lastNoteEndPos = osNote.StartPos + osNote.Length;
                lastNoteKeyNumber = osNote.KeyNumber;
            }
            //转换音高曲线
            encodePitch(ustxVoicePart, ustxProject, osTrack.EditedParams.Pitch.PointList);

            return ustxVoicePart;
        }
        public static UWavePart EncodeWavePart(InstrumentalTrack osTrack, int trackNo)
        {
            UWavePart ustxWavePart = new UWavePart
            {
                name = osTrack.Title,
                trackNo = trackNo,
                position = osTrack.Offset,
                FilePath = osTrack.AudioFilePath,
                relativePath = osTrack.AudioFilePath
            };
            return ustxWavePart;
        }
        public static UNote EncodeNote(Note osNote,bool snapFirst,int lastNoteKeyNumber)
        {
            //snapFirst：是否与上一个音符挨着，挨着就是True
            //lastNoteKeyNumber：上一个音符的音高
            int Y0 = 0;
            if(snapFirst==true)
            {
                Y0=(lastNoteKeyNumber-osNote.KeyNumber)*10;
            }
            string lyric = osNote.Lyric;
            if(osNote.Pronunciation != null)//如果有发音，则用发音
            {
                lyric=osNote.Pronunciation;
            }
            if("-" ==  lyric)//OpenUTAU中的连音符为+
            {
                lyric = "+";
            }
            if(lyric.Length==2 && LyricUtil.isPunctuation(lyric[1]))//删除标点符号
            {
                lyric = lyric.Remove(1);
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
                        new PitchPoint {X=-40,Y=Y0,shape=PitchPointShape.io},
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
        public static void encodePitch(UVoicePart part, UProject project, List<Tuple<int, int>> osPitch)
        {
            int pitchStart = BasePitchGenerator.pitchStart;
            int pitchInterval = BasePitchGenerator.pitchInterval;

            float[] basePitch = BasePitchGenerator.BasePitch(part, project);//生成基础音高线

            int firstBarLength = 1920;
            int osPitchPointer = 0;//当前位于输入osPitch的第几个采样点
            UCurve pitd = new UCurve
            {
                abbr = "pitd",
            };
            for (int i = 0; i < basePitch.Count(); i++)
            {
                int time = i * pitchInterval + pitchStart;//当前openutau采样点的时间，以tick为单位，从0开始
                while (osPitch[osPitchPointer+1].Item1 <= time+firstBarLength)
                {
                    osPitchPointer++;
                }
                //此时，osPitchPointer对应的位置恰好在time之前(或等于)，区间[osPitchPointer,osPitchPointer+1)包含time
                if (osPitch[osPitchPointer].Item2 < 0)//间断点
                {
                    pitd.xs.Add(time);
                    pitd.ys.Add(0);
                }
                else//有实际曲线存在
                {
                    int x1 = osPitch[osPitchPointer].Item1-firstBarLength;
                    int x2 = osPitch[osPitchPointer+1].Item1 - firstBarLength;
                    int y1 = osPitch[osPitchPointer].Item2;
                    int y2 = osPitch[osPitchPointer+1].Item2;
                    pitd.xs.Add(time);
                    pitd.ys.Add((y2 - y1) * (time - x1) / (x2 - x1) + y1 - (int)basePitch[i]);//线性插值
                }
            }
            part.curves.Add(pitd);
        }
    }
}