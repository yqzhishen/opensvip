using System;
using System.Linq;
using OxygenDioxide.UstxPlugin.Ustx;
using OxygenDioxide.UstxPlugin.Utils;

//根据音符、锚点和颤音，生成基础音高线
namespace OxygenDioxide.UstxPlugin.Stream
{ 
    public static class BasePitchGenerator
    {
        public static int pitchStart = 0;
        public static int pitchInterval = 5;
        public static float[] BasePitch(UVoicePart part, UProject project)
        {
            var timeAxis = new TimeAxis();
            timeAxis.BuildSegments(project);
            var uNotes = part.notes;//音符列表
            UNote prevNote = null;

            //============================================
            float[] pitches = new float[Math.Max(part.Duration, part.notes.Last().End) / pitchInterval];//音高线长度。音高线终点为结尾音素的末端
            int index = 0;
            foreach (var note in uNotes)
            {
                //如果和上一个音符之间断开，则断开部分的第一个采样点与上一音符连续
                if(pitchStart + index * pitchInterval < note.position && index < pitches.Length && index>0)
                {
                    pitches[index] = pitches[index - 1];
                    index++;
                }
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
            foreach (var note in uNotes)//对每个音符
            {
                if (note.vibrato.length <= 0)
                {//如果音符的颤音长度<=0，则无颤音。颤音长度按毫秒存储
                    continue;
                }
                int startIndex = Math.Max(0, (int)Math.Ceiling((float)(note.position - pitchStart) / pitchInterval));//音符起点在采样音高线上的x坐标
                int endIndex = Math.Min(pitches.Length, (note.End - pitchStart) / pitchInterval);//音符终点在采样音高线上的x坐标
                //使用音符开头的曲速计算颤音周期
                //float nPeriod = (float)(note.vibrato.period / note.DurationMs);
                float nPeriod = (float)(note.vibrato.period / timeAxis.MsBetweenTickPos(note.position, note.End));
                for (int i = startIndex; i < endIndex; ++i)//对每个采样点
                {
                    float nPos = (float)(pitchStart + i * pitchInterval - note.position) / note.duration;//音符长度，单位为5tick
                    var point = note.vibrato.Evaluate(nPos, nPeriod, note);//将音符长度颤音长度代入进去，求出带颤音的音高线
                    pitches[i] = point.Y * 100;
                }
            }
            foreach (var note in uNotes)
            {//对每个音符
                var pitchPoints = note.pitch.data//音高控制点
                    //OpenUTAU的控制点按毫秒存储（这个设计会导致修改曲速时出现混乱），这里先转成tick
                    .Select(point => {
                        double nodePosMs = timeAxis.TickPosToMsPos(part.position + note.position);
                        return new PitchPoint(
                               timeAxis.MsPosToTickPos(nodePosMs + point.X) - part.position,
                               point.Y * 10 + note.tone * 100,
                               point.shape);
                    })
                    .ToList();
                if (pitchPoints.Count() == 0)
                {//如果没有控制点，则默认台阶形
                    pitchPoints.Add(new PitchPoint(note.position, note.tone * 100));
                    pitchPoints.Add(new PitchPoint(note.End, note.tone * 100));
                }
                if (note == uNotes.First() && pitchPoints[0].X > pitchStart)
                {//如果整个段落开头有控制点没覆盖到的地方（以音素开头为准），则向前水平延伸
                    pitchPoints.Insert(0, new PitchPoint(pitchStart, pitchPoints[0].Y));
                }
                else if (pitchPoints[0].X > note.position)
                {//对于其他音符，则以卡拍点为准
                    pitchPoints.Insert(0, new PitchPoint(note.position, pitchPoints[0].Y));
                }
                if (pitchPoints.Last().X < note.End)
                {//如果整个段落结尾有控制点没覆盖到的地方，则向后水平延伸
                    pitchPoints.Add(new PitchPoint(note.End, pitchPoints.Last().Y));
                }
                PitchPoint prevPoint = pitchPoints[0];//现在lastpoint是第一个控制点
                //起点在采样音高线上的x坐标，以5tick为单位。如果第一个控制点在0前面，就从0开始，否则从第一个控制点开始
                index = Math.Max(0, (int)((prevPoint.X - pitchStart) / pitchInterval));
                foreach (var point in pitchPoints.Skip(1))
                {//对每一段曲线
                    int x = pitchStart + index * pitchInterval;//起点在工程中的x坐标
                    while (x <= point.X && index < pitches.Length)
                    {//遍历采样音高点
                        float pitch = (float)MusicMath.InterpolateShape(prevPoint.X, point.X, prevPoint.Y, point.Y, x, prevPoint.shape);//绝对音高。插值，正式将控制点转化为曲线！
                        float basePitch = prevNote != null && x < prevNote.End
                            ? prevNote.tone * 100
                            : note.tone * 100;//台阶基础音高
                        pitches[index] += pitch - basePitch;//锚点音高比基础音高高了多少
                        index++;
                        x += pitchInterval;
                    }
                    prevPoint = point;
                }
                prevNote = note;
            }
            return pitches;
        }
    }
}