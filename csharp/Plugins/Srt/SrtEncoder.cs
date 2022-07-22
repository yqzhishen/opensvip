using FlutyDeer.SrtPlugin.Options;
using FlutyDeer.SrtPlugin.Utils;
using OpenSvip.Library;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.SrtPlugin
{
    public class SrtEncoder
    {
        public SplitByOption SplitByOption { get; set; }

        private Project osProject;

        public int Offset { get; set; }

        private TimeSynchronizer timeSynchronizer;

        public SrtFile EncodeProject(Project project)
        {
            SrtFile srtFile = new SrtFile();
            osProject = project;
            timeSynchronizer = new TimeSynchronizer(osProject.SongTempoList);
            var firstBarTimeSignature = osProject.TimeSignatureList[0];
            var singsingTrack = osProject.TrackList.Where(t => t is SingingTrack).First() as SingingTrack;
            var noteList = singsingTrack.NoteList;
            var buffer = new List<Note>();
            for (int i = 0; i < noteList.Count; i++)
            {
                var note = noteList[i];
                buffer.Add(note);
                int currentNoteEndPos = note.StartPos + note.Length;
                bool commitFlag = false;
                bool conditionSymbol = LyricsUtil.ContainsSymbol(note.Lyric);
                bool conditionGap = i < noteList.Count - 1 && noteList[i + 1].StartPos - currentNoteEndPos >= 60;
                switch (SplitByOption)
                {
                    case SplitByOption.Symbol:
                        commitFlag = conditionSymbol;
                        break;
                    case SplitByOption.Gap:
                        commitFlag = conditionGap;
                        break;
                    case SplitByOption.Both:
                        commitFlag = conditionSymbol || conditionGap;
                        break;
                }
                if (i == noteList.Count - 1)
                {
                    commitFlag = true;
                }
                if (commitFlag)
                {
                    CommitCurrentLyricLine(srtFile, buffer);
                }
            }
            foreach (var line in srtFile.SrtItems)
            {
                line.StartTime -= TimeSpan.FromMilliseconds(Offset);
                line.EndTime -= TimeSpan.FromMilliseconds(Offset);
            }
            return srtFile;
        }

        private void CommitCurrentLyricLine(SrtFile srtFile, List<Note> buffer)
        {
            int startPos = buffer.First().StartPos;
            int endPos = buffer.Last().StartPos + buffer.Last().Length;
            var startTime = GetTimeSpanFromTicks(startPos);
            var endTime = GetTimeSpanFromTicks(endPos);
            var lyric = "";
            foreach (var note in buffer)
            {
                var currentLyric = LyricsUtil.GetSymbolRemovedLyric(note.Lyric);
                lyric += currentLyric;
            }
            var srtItem = new SrtItem
            {
                StartTime = startTime,
                EndTime = endTime,
                Lyric = lyric
            };
            srtFile.Add(srtItem);
            buffer.Clear();
        }

        //把秒转换为时分秒格式
        public TimeSpan GetTimeSpanFromTicks(int tick)
        {
            var secs = timeSynchronizer.GetActualSecsFromTicks(tick);
            var time = TimeSpan.FromSeconds(secs);
            return time;
        }
    }
}
