using System;
using System.Collections.Generic;
using System.Linq;
using OpenSvip.Model;
using OxygenDioxide.UstxPlugin.Ustx;
using OxygenDioxide.UstxPlugin.Utils;

namespace OxygenDioxide.UstxPlugin.Stream
{
    public static class UstxEncoder
    {
        public static UProject EncodeProject(Project osProject)
        {
            //节拍
            List<UTimeSignature> ustxTimeSignatures = osProject.TimeSignatureList
                .Select(EncodeTimeSignature)
                .ToList();
            if (ustxTimeSignatures.Count == 0)
            {
                ustxTimeSignatures.Add(new UTimeSignature(0, 4, 4));
            }
            var firstBarLength = 1920 * ustxTimeSignatures[0].beatPerBar / ustxTimeSignatures[0].beatUnit;

            //曲速
            List<UTempo> tempos = osProject.SongTempoList
                .Select(x=>EncodeTempo(x,firstBarLength))
                .ToList();
            if(tempos.Count==0)
            {
                tempos.Add(new UTempo
                {
                    bpm = 120,
                    position = 0,
                });
            }

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
                    ustxProject.voiceParts.Add(EncodeVoicePart((SingingTrack)osTrack,trackNo,ustxProject,firstBarLength));
                }
                else//伴奏音轨
                {
                    ustxProject.waveParts.Add(EncodeWavePart((InstrumentalTrack)osTrack,trackNo));
                }
                trackNo += 1;
            }
            return ustxProject;
        }

        public static UTempo EncodeTempo(SongTempo osTempo, int firstBarLength = 1920)
        {
            return new UTempo
            {
                position = Math.Max(osTempo.Position - firstBarLength, 0),
                bpm = osTempo.BPM
            };
        }

        public static UTimeSignature EncodeTimeSignature(TimeSignature osTimeSignature)
        {
            return new UTimeSignature
            {
                barPosition = osTimeSignature.BarIndex,
                beatPerBar = osTimeSignature.Numerator,
                beatUnit = osTimeSignature.Denominator
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
        
        public static UVoicePart EncodeVoicePart(SingingTrack osTrack, int trackNo, UProject ustxProject, int firstBarLength = 1920)
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
            EncodePitch(ustxVoicePart, ustxProject, osTrack.EditedParams.Pitch.PointList, firstBarLength);

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
            string lyric = LyricsUtil.GetSymbolRemovedLyric(osNote.Lyric);//去除标点符号
            if(osNote.Pronunciation != null)//如果有发音，则用发音
            {
                lyric=osNote.Pronunciation;
            }
            if("-" ==  lyric)//OpenUTAU中的连音符为+
            {
                lyric = "+";
            }
            if(lyric.Length==2 && LyricsUtil.isPunctuation(lyric[1]))//删除标点符号
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
        
        public static void EncodePitch(UVoicePart part, UProject project, List<Tuple<int, int>> osPitch, int firstBarLength = 1920)
        {        
            int pitchStart = BasePitchGenerator.pitchStart;
            int pitchInterval = BasePitchGenerator.pitchInterval;
            float[] basePitch = BasePitchGenerator.BasePitch(part, project);//生成基础音高线
            int pitchEndX = basePitch.Count() * pitchInterval + firstBarLength + 1;

            //如果osPitch为空
            if (osPitch == null || osPitch.Count == 0)
            {
                osPitch = new List<Tuple<int, int>>() { Tuple.Create(0, -1), Tuple.Create(pitchEndX, -1) };
            }
            //如果osPitch末尾不完整
            if (osPitch.Last().Item1 < pitchEndX)
            {
                osPitch.Add(Tuple.Create(osPitch.Last().Item1, -1));
                osPitch.Add(Tuple.Create(pitchEndX, -1));
            }

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
                    int x1 = osPitch[osPitchPointer].Item1 - firstBarLength;
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