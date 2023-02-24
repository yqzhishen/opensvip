using FlutyDeer.LyricsPlugin.Options;
using FlutyDeer.LyricsPlugin.Utils;
using Newtonsoft.Json;
using OpenSvip.Library;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlutyDeer.LyricsPlugin
{
    public class LyricsEncoder
    {
        public string Artist { get; set; }
        
        public string Title { get; set; }
        
        public string Album { get; set; }

        public string By { get; set; }
        
        public int Offset { get; set; }

        public OffsetPolicyOption OffsetPolicyOption { get; set; }

        public SplitByOption SplitByOption { get; set; }

        public string lyricsText { get; set; }

        public int autoInsertBlankLine { get; set; }

        private Project osProject;

        private TimeSynchronizer timeSynchronizer;
        
        public LyricsFile EncodeProject(Project project)
        {
            LyricsFile lyricsFile = new LyricsFile();
            osProject = project;
            timeSynchronizer = new TimeSynchronizer(osProject.SongTempoList);
            var firstBarTimeSignature = osProject.TimeSignatureList[0];
            var singsingTrack = osProject.TrackList.Where(t => t is SingingTrack).First() as SingingTrack;
            var noteList = singsingTrack.NoteList;
            var buffer = new List<Tuple<int, string>>();
            var lyricsBindedUnitSeries = new LyricsReference.LyricsBindedUnit[0];
            if(SplitByOption == SplitByOption.Text) {
                var lyricsNotePinyinSeries = new string[noteList.Count];
                var lyricsReference = new LyricsReference(lyricsText);
                lyricsBindedUnitSeries = lyricsReference.generateLyricBindedUnitSeries(noteList);
            }
            for (int i = 0; i < noteList.Count; i++)
            {
                var note = noteList[i];
                var lyricOfNote = SplitByOption == SplitByOption.Text ? lyricsBindedUnitSeries[i].hanzi : note.Lyric;
                buffer.Add(new Tuple<int, string>(note.StartPos, lyricOfNote));
                int currentNoteEndPos = note.StartPos + note.Length;
                bool commitFlag = false;
                bool conditionSymbol = LyricsUtil.ContainsSymbol(note.Lyric);
                bool conditionGap = i < noteList.Count - 1 && noteList[i + 1].StartPos - currentNoteEndPos >= 60;
                bool blankLineFlag = autoInsertBlankLine >= 0 && i < noteList.Count - 1 && noteList[i + 1].StartPos - currentNoteEndPos >= 480 * autoInsertBlankLine;
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
                    case SplitByOption.Text:
                        commitFlag = lyricsBindedUnitSeries[i].positionInLine == LyricsReference.PositionInLine.END || (i < noteList.Count - 1 && lyricsBindedUnitSeries[i + 1].positionInLine == LyricsReference.PositionInLine.START);
                        break;
                }
                if (i == noteList.Count - 1)
                {
                    commitFlag = true;
                }
                commitFlag = commitFlag || blankLineFlag;
                if (commitFlag)
                {
                    CommitCurrentLyricLine(lyricsFile, buffer);
                }
                if(blankLineFlag) {
                    buffer.Add(new Tuple<int, string>(currentNoteEndPos, ""));
                    CommitCurrentLyricLine(lyricsFile, buffer);
                }
            }
            lyricsFile.AddMeta(new MetaInfoLine(MetaInfoType.Artist, Artist));
            lyricsFile.AddMeta(new MetaInfoLine(MetaInfoType.Title, Title));
            lyricsFile.AddMeta(new MetaInfoLine(MetaInfoType.Album, Album));
            lyricsFile.AddMeta(new MetaInfoLine(MetaInfoType.By, By));
            switch (OffsetPolicyOption)
            {
                case OffsetPolicyOption.Meta:
                    lyricsFile.AddMeta(new MetaInfoLine(MetaInfoType.Offset, Offset.ToString()));
                    break;
                case OffsetPolicyOption.Timeline:
                    foreach(var line in lyricsFile.LyricLines)
                    {
                        line.StartTime -= TimeSpan.FromMilliseconds(Offset);
                    }
                    break;
            }
            return lyricsFile;
        }

        private void CommitCurrentLyricLine(LyricsFile lyricsFile, List<Tuple<int, string>> buffer)
        {
            var time = GetTimeSpanFromTicks(buffer[0].Item1);
            var lyric = "";
            foreach (var item in buffer)
            {
                var currentLyric = LyricsUtil.GetSymbolRemovedLyric(item.Item2);
                lyric += currentLyric;
            }
            var lyricLine = new LyricLine
            {
                StartTime = time,
                Lyric = lyric
            };
            lyricsFile.AddLyric(lyricLine);
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
