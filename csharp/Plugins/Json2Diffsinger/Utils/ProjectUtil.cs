using System;
using System.Collections.Generic;
using System.Linq;
using OpenSvip.Library;
using OpenSvip.Model;

namespace Json2DiffSinger.Utils
{
    public static class ProjectUtil
    {
        /// <summary>
        /// 重设时间轴，转换为单一曲速的工程
        /// </summary>
        /// <param name="project"></param>
        /// <param name="tempo">默认 125，梯相当于毫秒值</param>
        public static void ResetTimeAxis(this Project project, int tempo = 125)
        {
            var synchronizer = new TimeSynchronizer(project.SongTempoList, isAbsoluteTimeMode: true, defaultTempo: tempo);
            foreach (var track in project.TrackList.OfType<SingingTrack>())
            {
                foreach (var note in track.NoteList)
                {
                    var end = (int) Math.Round(synchronizer.GetActualTicksFromTicks(note.StartPos + note.Length));
                    note.StartPos = (int) Math.Round(synchronizer.GetActualTicksFromTicks(note.StartPos));
                    note.Length = end - note.StartPos;
                }

                var firstBarTicks = 1920 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator;
                for (var i = 0; i < track.EditedParams.Pitch.TotalPointsCount; i++)
                {
                    var (pos, val) = track.EditedParams.Pitch.PointList[i];
                    pos = (int) Math.Round(synchronizer.GetActualTicksFromTicks(pos - firstBarTicks)) + 1920;
                    track.EditedParams.Pitch.PointList[i] = new Tuple<int, int>(pos, val);
                }
                for (var i = 0; i < track.EditedParams.Gender.TotalPointsCount; i++)
                {
                    var (pos, val) = track.EditedParams.Gender.PointList[i];
                    pos = (int)Math.Round(synchronizer.GetActualTicksFromTicks(pos - firstBarTicks)) + 1920;
                    track.EditedParams.Gender.PointList[i] = new Tuple<int, int>(pos, val);
                }
            }

            project.SongTempoList = new List<SongTempo>
            {
                new SongTempo
                {
                    BPM = tempo, Position = 0
                }
            };
            project.TimeSignatureList = new List<TimeSignature>
            {
                new TimeSignature
                {
                    BarIndex = 0, Numerator = 4, Denominator = 4
                }
            };
        }

        /// <summary>
        /// 按时间间隔将音符序列切割为多个分段。
        /// </summary>
        /// <param name="project"></param>
        /// <param name="minInterval">切割间隔阈值（毫秒）</param>
        /// <param name="minLength">最短分段长度（毫秒）</param>
        /// <returns>IEnumerable of (offset, project, trailingSpace)</returns>
        public static IEnumerable<(double, Project, float)> SplitIntoSegments(
            this Project project,
            int minInterval = 400,
            int minLength = 5000)
        {
            var track = project.TrackList.OfType<SingingTrack>().FirstOrDefault();
            if (track == null || !track.NoteList.Any())
            {
                return Array.Empty<(double, Project, float)>();
            }
            
            project.ResetTimeAxis();
            var result = new List<(double, Project, float)>();
            var buffer = new List<Note>
            {
                track.NoteList.First()
            };
            
            var curSegStart = Math.Max(track.NoteList.First().StartPos - 600, (int) (track.NoteList.First().StartPos * 0.8));
            var curSegInterval = track.NoteList.First().StartPos;
            for (var i = 1; i < track.NoteList.Count; ++i)
            {
                var prev = track.NoteList[i - 1];
                var cur = track.NoteList[i];
                var interval = cur.StartPos - prev.StartPos - prev.Length;
                if (interval >= minInterval && cur.StartPos - interval * 0.8 - curSegStart >= minLength)
                {
                    var prepareSpace = Math.Min(600, (int)(curSegInterval * 0.8));
                    var trailingSpace = Math.Min(400, (int)(curSegInterval * 0.2));
                    var segNoteStartPos = buffer.First().StartPos;
                    buffer.ForEach(note => note.StartPos = note.StartPos - segNoteStartPos + prepareSpace);
                    var pitchPoints = track.EditedParams.Pitch.PointList
                        .Select(point => new Tuple<int, int>(point.Item1 - segNoteStartPos + prepareSpace, point.Item2))
                        .Where(point =>
                            point.Item1 >= 1920
                            && point.Item1 - 1920 <= buffer.Last().StartPos + buffer.Last().Length + 50)
                        .ToList();
                    var genderPoints = track.EditedParams.Gender.PointList
                        .Select(point => new Tuple<int, int>(point.Item1 - segNoteStartPos + prepareSpace, point.Item2))
                        .Where(point =>
                            point.Item1 >= 1920
                            && point.Item1 - 1920 <= buffer.Last().StartPos + buffer.Last().Length + 50)
                        .ToList();

                    curSegStart = cur.StartPos - Math.Min(600, (int)(interval * 0.8));
                    curSegInterval = interval;
                    var segment = new Project
                    {
                        SongTempoList = project.SongTempoList,
                        TimeSignatureList = new List<TimeSignature>
                        {
                            new TimeSignature
                            {
                                BarIndex = 0, Numerator = 4, Denominator = 4
                            }
                        },
                        TrackList = new List<Track>(1)
                        {
                            new SingingTrack
                            {
                                NoteList = buffer,
                                EditedParams = new Params
                                {
                                    Pitch = new ParamCurve
                                    {
                                        PointList = pitchPoints
                                    },
                                    Gender = new ParamCurve
                                    {
                                        PointList = genderPoints
                                    }
                                }
                            }
                        }
                    };
                    result.Add(((segNoteStartPos - prepareSpace) / 1000.0, segment, trailingSpace / 1000.0f));
                    
                    buffer = new List<Note>();
                }
                buffer.Add(cur);
            }
            
            {
                var prepareSpace = Math.Min(600, (int) (curSegInterval * 0.8));
                var segNoteStartPos = buffer.First().StartPos;
                buffer.ForEach(note => note.StartPos = note.StartPos - segNoteStartPos + prepareSpace);
                var pitchPoints = track.EditedParams.Pitch.PointList
                    .Select(point => new Tuple<int, int>(point.Item1 - segNoteStartPos + prepareSpace, point.Item2))
                    .Where(point =>
                        point.Item1 >= 1920
                        && point.Item1 - 1920 <= buffer.Last().StartPos + buffer.Last().Length + 50)
                    .ToList();
                var genderPoints = track.EditedParams.Gender.PointList
                    .Select(point => new Tuple<int, int>(point.Item1 - segNoteStartPos + prepareSpace, point.Item2))
                    .Where(point =>
                        point.Item1 >= 1920
                        && point.Item1 - 1920 <= buffer.Last().StartPos + buffer.Last().Length + 50)
                    .ToList();

                var segment = new Project
                {
                    SongTempoList = project.SongTempoList,
                    TimeSignatureList = new List<TimeSignature>
                    {
                        new TimeSignature
                        {
                            BarIndex = 0, Numerator = 4, Denominator = 4
                        }
                    },
                    TrackList = new List<Track>(1)
                    {
                        new SingingTrack
                        {
                            NoteList = buffer,
                            EditedParams = new Params
                            {
                                Pitch = new ParamCurve
                                {
                                    PointList = pitchPoints
                                },
                                Gender = new ParamCurve
                                {
                                    PointList = genderPoints
                                }
                            }
                        }
                    }
                };
                result.Add(((segNoteStartPos - prepareSpace) / 1000.0, segment, 0.5f));
            }

            return result;
        }
    }
}
