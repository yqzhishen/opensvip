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
        LyricUtil lyricUtil = new LyricUtil();

        public UProject EncodeProject(Project osProject)
        {
            double bpm = 120.0;//如果对象没有给出曲速，则默认120
            int beatPerBar = 4;//如果对象没有给出节拍号，则默认4/4
            int beatUnit = 4;
            if(osProject.SongTempoList.Count()>=1)
            {
                bpm = osProject.SongTempoList[0].BPM;//OpenUTAU不支持变速，这里采用音轨开头的bpm，曲速转换功能待开发
            }
            if(osProject.TimeSignatureList.Count()>=1)
            {
                beatPerBar = osProject.TimeSignatureList[0].Numerator;
                beatUnit = osProject.TimeSignatureList[0].Denominator;
            }
            UProject ustxProject = new UProject {
                bpm = bpm,
                beatPerBar = beatPerBar,
                beatUnit = beatUnit,
                voiceParts = new List<UVoicePart>(),
                waveParts = new List<UWavePart>(),
                ustxVersion = new System.Version("0.5")
                
            };
            int trackNo = 0;
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
        public UVoicePart EncodeVoicePart(SingingTrack osTrack, int trackNo, UProject ustxProject)
        {
            UVoicePart ustxVoicePart = new UVoicePart {
                name = osTrack.Title,
                trackNo = trackNo,
                position = 0
            };
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
        public UWavePart EncodeWavePart(InstrumentalTrack osTrack, int trackNo)
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
            if(osNote.Pronunciation != null)//如果有发音，则用发音
            {
                lyric=osNote.Pronunciation;
            }
            if("-" ==  lyric)//OpenUTAU中的连音符为+
            {
                lyric = "+";
            }
            if(lyric.Length==2 && lyricUtil.isHanzi(lyric[0]) && lyricUtil.isPunctuation(lyric[1]))//删除标点符号
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
        public void encodePitch(UVoicePart part, UProject project, List<Tuple<int, int>> osPitch)
        {
            var pitchGenerator = new BasePitchGenerator();
            int pitchStart = pitchGenerator.pitchStart;
            int pitchInterval = pitchGenerator.pitchInterval;

            float[] basePitch = new BasePitchGenerator().BasePitch(part, project);//生成基础音高线

            int firstBarLength = 1920 * project.beatPerBar / project.beatUnit;
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
            //#TODO：调试
        }
    }
}