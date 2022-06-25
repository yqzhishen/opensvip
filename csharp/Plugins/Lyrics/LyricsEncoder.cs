using FlutyDeer.MidiPlugin.Utils;
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
        private Project osProject;
        
        private string lrcContent = "";

        private int firstBarLength;

        private TimeSynchronizer timeSynchronizer;
        
        public string EncodeProject(Project project)
        {
            osProject = project;
            timeSynchronizer = new TimeSynchronizer(osProject.SongTempoList);
            var firstBarTimeSignature = osProject.TimeSignatureList[0];
            firstBarLength = 1920 * firstBarTimeSignature.Numerator / firstBarTimeSignature.Denominator;
            var singsingTrack = osProject.TrackList.Where(t => t is SingingTrack).First() as SingingTrack;
            var noteList = singsingTrack.NoteList;
            var buffer = new List<Tuple<int, string>>();//tick:lyric
            for (int i = 0; i < noteList.Count; i++)
            {
                var note = noteList[i];
                buffer.Add(new Tuple<int, string>(note.StartPos, note.Lyric));
                int currentNoteEndPos = note.StartPos + note.Length;
                if ((i < noteList.Count - 1 && currentNoteEndPos != noteList[i + 1].StartPos) || i == noteList.Count - 1 || note.Lyric.Contains("。"))
                {
                    var time = ConvertTicksToFormattedTime(buffer[0].Item1);
                    var lyricLine = new LyricLine();
                    lyricLine.Time = time;
                    foreach (var item in buffer)
                    {
                        var currentLyric = LyricsUtil.GetSymbolRemovedLyric(item.Item2);
                        lyricLine.Lyric += currentLyric;
                    }
                    lrcContent += lyricLine.ToString() + "\n";
                    buffer.Clear();
                }
            }
            return lrcContent;
        }
        //把秒转换为时分秒格式
        public string ConvertTicksToFormattedTime(int tick)
        {
            var secs = timeSynchronizer.GetActualSecsFromTicks(tick);
            var time = TimeSpan.FromSeconds(secs);
            return $"{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
        }
    }
}
