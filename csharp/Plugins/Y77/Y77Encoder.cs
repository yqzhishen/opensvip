using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.Y77Plugin
{
    public class Y77Encoder
    {
        private Project osProject;

        private int noteCount = 0;

        private int firstBarLength = 0;

        public Y77Project EncodeProject(Project project)
        {
            osProject = project;
            firstBarLength = 1920 * GetNumerator(0) / GetDenominator(0);
            Y77Project y77Project = new Y77Project
            {
                BarCount = 100,
                NoteList = EncodeNoteList(),
                NoteCount = noteCount,
                BPM = osProject.SongTempoList[0].BPM,
                TimeSignatureNumerator = GetNumerator(0),
                TimeSignatureDenominator = GetDenominator(0)
            };
            return y77Project;
        }

        private List<Y77Note> EncodeNoteList()
        {
            List<Y77Note> y77NoteList = new List<Y77Note>();
            foreach (var track in osProject.TrackList)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        foreach (var note in singingTrack.NoteList)
                        {
                            y77NoteList.Add(EncodeNote(singingTrack, note));
                        }
                        break;
                    default:
                        break;
                }
                break;
            }
            noteCount = y77NoteList.Count;
            return y77NoteList;
        }

        private Y77Note EncodeNote(SingingTrack singingTrack, Note note)
        {
            Y77Note y77Note = new Y77Note
            {
                Pinyin = note.Pronunciation,
                Lyric = note.Lyric,
                KeyNumber = 88 - note.KeyNumber,
                PitchParam = EncodePitchParam(singingTrack.EditedParams.Pitch, note)
            };
            y77Note.StartPosition = note.StartPos / 30;
            y77Note.Length = (note.StartPos + note.Length) / 30 - y77Note.StartPosition;
            return y77Note;
        }

        private List<int> EncodePitchParam(ParamCurve pitchParamCurve, Note note)
        {
            List<int> sampleTimeList = new List<int>();//采样时间点列表
            for (int i = 0; i < 500; i++)
            {
                sampleTimeList.Add(note.StartPos + firstBarLength + (int)((note.Length / 500.0) * i));
            }
            var pitchParamInNote = pitchParamCurve.PointList.Where(p => p.Item1 >= note.StartPos + firstBarLength && p.Item1 <= note.StartPos + firstBarLength + note.Length).ToList();

            List<int> pitchParamTimeInNote = new List<int>();
            foreach (var paramPoint in pitchParamInNote)
            {
                pitchParamTimeInNote.Add(paramPoint.Item1);
            }

            List<int> y77PitchParam = new List<int>();
            foreach (var sampleTime in sampleTimeList)
            {
                if (pitchParamTimeInNote.Contains(sampleTime))
                {
                    var pitch = pitchParamCurve.PointList.Where(p => p.Item1 == sampleTime).First().Item2;
                    if (pitch == -100)
                    {
                        y77PitchParam.Add(50);
                    }
                    else
                    {
                        var value = 50 + (pitch - note.KeyNumber * 100) / 2;
                        y77PitchParam.Add(value);
                    }
                }
                else
                {
                    var distance = -1;
                    var value = 50;

                    foreach (var point in pitchParamInNote)
                    {
                        if (distance > Math.Abs(point.Item1 - sampleTime) || distance == -1)
                        {
                            distance = Math.Abs(point.Item1 - sampleTime);
                            value = 50 + (point.Item2 - note.KeyNumber * 100) / 2;
                        }
                    }
                    y77PitchParam.Add(value);
                }
            }
            var buffer = new List<int>();
            int previousNode = y77PitchParam[0];
            int previousNodeIndex = 0;
            for (int i = 0; i < y77PitchParam.Count(); i++)
            {
                if (y77PitchParam[i] == previousNode)
                {
                    buffer.Add(y77PitchParam[i]);
                }
                else
                {
                    int currentNode = y77PitchParam[i];
                    for (int j = 0; j < buffer.Count(); j++)//插值
                    {
                        y77PitchParam[previousNodeIndex + j] = previousNode + j * (y77PitchParam[i] - buffer[j]) / buffer.Count();
                    }
                    buffer.Clear();
                }
                if (y77PitchParam[i] != previousNode)
                {
                    previousNodeIndex = i;
                    previousNode = y77PitchParam[i];
                }
            }
            return y77PitchParam;
        }

        /// <summary>
        /// 返回拍号的分子。
        /// </summary>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        private int GetNumerator(int index)
        {
            return osProject.TimeSignatureList[index].Numerator;
        }

        /// <summary>
        /// 返回拍号的分母。
        /// </summary>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        private int GetDenominator(int index)
        {
            return osProject.TimeSignatureList[index].Denominator;
        }
    }
}
