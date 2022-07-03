using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenSvip.Model;

using OpenUtau.Core.Ustx;
using OpenUtau.Core;
using Microsoft.SqlServer.Server;

namespace OxygenDioxide.UstxPlugin.Stream
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
                DecodeVoicePart(ustxVoicePart,(SingingTrack)trackList[ustxVoicePart.trackNo],ustxProject);
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
            SingingTrack osTrack = new SingingTrack
            {
                Title = "",
                Mute = ustxTrack.Mute,
                Solo = ustxTrack.Solo,
                Volume = Math.Min(Math.Pow(10, ustxTrack.Volume / 10), 2.0),//OpenUTAU的音量以分贝存储（#TODO:待实测），这里转为绝对音量
                Pan = 0,
                AISingerName = ustxTrack.singer,
            };
            osTrack.EditedParams.Pitch = new ParamCurve
            {
                PointList = new List<Tuple<int, int>>
                {
                }
            };
            return osTrack;
        }
        public void DecodeVoicePart(UVoicePart ustxVoicePart, SingingTrack osTrack, UProject ustxProject)
        {
            int partOffset = ustxVoicePart.position;
            foreach (UNote ustxNote in ustxVoicePart.notes)
            {
                osTrack.NoteList.Add(DecodeNote(ustxNote,partOffset));
            }
            osTrack.EditedParams.Pitch.PointList.AddRange(DecodePitch(ustxVoicePart, ustxProject));
            if(osTrack.Title=="")
            {
                osTrack.Title = ustxVoicePart.name;//音轨名称采用第一个区段的名称
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
        public List<Tuple<int,int>> DecodePitch(UVoicePart part, UProject project)
        {
            int pitchStart;
            var uNotes = part.notes;//音符列表
            UNote prevNote = null;

            //============================================
            const int pitchInterval = 5;//每5tick一个音高点
            pitchStart = 0;//音高线起点：0
            float[] pitches = new float[Math.Max(part.Duration, part.notes.Last().End)/pitchInterval];//音高线长度。音高线终点为结尾音素的末端
            int index = 0;
            foreach (var note in uNotes)
            {
                while (pitchStart + index * pitchInterval < note.End && index < pitches.Length)
                {
                    pitches[index] = note.tone * 100;
                    index++;
                }//基础音高线为阶梯，只管当前处于哪个音符
            }
            index = Math.Max(1, index);
            while (index < pitches.Length)
            {
                pitches[index] = pitches[index - 1];//结尾如果还有多余的地方，就用最后一个音符的音高填充
                index++;
            }
            foreach (var note in uNotes)
            {//对每个音符
                if (note.vibrato.length <= 0)
                {//如果音符的颤音长度<=0，则无颤音。颤音长度按毫秒存储
                    continue;
                }
                int startIndex = Math.Max(0, (int)Math.Ceiling((float)(note.position - pitchStart) / pitchInterval));//音符起点在采样音高线上的x坐标
                int endIndex = Math.Min(pitches.Length, (note.End - pitchStart) / pitchInterval);//音符终点在采样音高线上的x坐标
                for (int i = startIndex; i < endIndex; ++i)
                {
                    float nPos = (float)(pitchStart + i * pitchInterval - note.position) / note.duration;//音符长度，单位为5tick
                    float nPeriod = (float)project.MillisecondToTick(note.vibrato.period) / note.duration;//颤音长度，单位为5tick
                    var point = note.vibrato.Evaluate(nPos, nPeriod, note);//将音符长度颤音长度代入进去，求出带颤音的音高线
                    pitches[i] = point.Y * 100;
                }
            }
            foreach (var note in uNotes)
            {//对每个音符
                var pitchPoints = note.pitch.data//音高控制点
                    .Select(point => new PitchPoint(//OpenUTAU的控制点按毫秒存储（这个设计会导致修改曲速时出现混乱），这里先转成tick
                        project.MillisecondToTick(point.X) + note.position,
                        point.Y * 10 + note.tone * 100,
                        point.shape))
                    .ToList();
                if (pitchPoints.Count == 0)
                {//如果没有控制点，则默认台阶形
                    pitchPoints.Add(new PitchPoint(note.position, note.tone * 100));
                    pitchPoints.Add(new PitchPoint(note.End, note.tone * 100));
                }
                if (note == uNotes.First() && pitchPoints[0].X > pitchStart)
                {
                    pitchPoints.Insert(0, new PitchPoint(pitchStart, pitchPoints[0].Y));//如果整个段落开头有控制点没覆盖到的地方（以音素开头为准），则向前水平延伸
                }
                else if (pitchPoints[0].X > note.position)
                {
                    pitchPoints.Insert(0, new PitchPoint(note.position, pitchPoints[0].Y));//对于其他音符，则以卡拍点为准
                }
                if (pitchPoints.Last().X < note.End)
                {
                    pitchPoints.Add(new PitchPoint(note.End, pitchPoints.Last().Y));//如果整个段落结尾有控制点没覆盖到的地方，则向后水平延伸
                }
                PitchPoint lastPoint = pitchPoints[0];//现在lastpoint是第一个控制点
                index = Math.Max(0, (int)((lastPoint.X - pitchStart) / pitchInterval));//起点在采样音高线上的x坐标，以5tick为单位。如果第一个控制点在0前面，就从0开始，否则从第一个控制点开始
                foreach (var point in pitchPoints.Skip(1))
                {//对每一段曲线
                    int x = pitchStart + index * pitchInterval;//起点在工程中的x坐标
                    while (x < point.X && index < pitches.Length)
                    {//遍历采样音高点
                        float pitch = (float)MusicMath.InterpolateShape(lastPoint.X, point.X, lastPoint.Y, point.Y, x, lastPoint.shape);//绝对音高。插值，正式将控制点转化为曲线！
                        float basePitch = prevNote != null && x < prevNote.End
                            ? prevNote.tone * 100
                            : note.tone * 100;//台阶基础音高
                        pitches[index] += pitch - basePitch;//锚点音高比基础音高高了多少
                        index++;
                        x += pitchInterval;
                    }
                    lastPoint = point;
                }
                prevNote = note;
            }

            var curve = part.curves.FirstOrDefault(c => c.abbr == "pitd");//PITD为手绘音高线差值。这里从ustx工程中尝试调取该参数
            if (curve != null && !curve.IsEmpty)
            {//如果参数存在
                for (int i = 0; i < pitches.Length; ++i)
                {
                    pitches[i] += curve.Sample(pitchStart + i * pitchInterval);//每个点加上PITD的值
                }
            }

            //============================================
            List<Tuple<int, int>> PointList = new List<Tuple<int, int>>
            {
            };
            int firstBarLength = 1920 * project.beatPerBar / project.beatUnit;
            PointList.Add(new Tuple<int, int>(firstBarLength + part.position,-100));
            for (int i = 0; i < pitches.Length; ++i)
            {
                PointList.Add(new Tuple<int, int>(firstBarLength + part.position + i * pitchInterval, (int)pitches[i]));
            }
            PointList.Add(new Tuple<int, int>(firstBarLength + part.position + pitches.Length * pitchInterval,-100));
            return PointList;
        }
    }
}