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
        public string Artist { get; set; }
        
        public string Title { get; set; }
        
        public string Album { get; set; }

        public string By { get; set; }
        
        public int Offset { get; set; }

        private Project osProject;

        private int firstBarLength;

        private TimeSynchronizer timeSynchronizer;
        
        public LyricsFile EncodeProject(Project project)
        {
            LyricsFile lyricsFile = new LyricsFile();
            lyricsFile.AddMeta(new MetaInfoLine(MetaInfoType.Artist, Artist));
            lyricsFile.AddMeta(new MetaInfoLine(MetaInfoType.Title, Title));
            lyricsFile.AddMeta(new MetaInfoLine(MetaInfoType.Album, Album));
            lyricsFile.AddMeta(new MetaInfoLine(MetaInfoType.By, By));
            lyricsFile.AddMeta(new MetaInfoLine(MetaInfoType.Offset, Offset.ToString()));
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
                if ((i < noteList.Count - 1 && noteList[i + 1].StartPos - currentNoteEndPos >= 60) || i == noteList.Count - 1)
                {
                    var time = ConvertTicksToFormattedTime(buffer[0].Item1);
                    var lyric = "";
                    foreach (var item in buffer)
                    {
                        var currentLyric = LyricsUtil.GetSymbolRemovedLyric(item.Item2);
                        lyric += currentLyric;
                    }
                    var lyricLine = new LyricLine
                    {
                        Time = time,
                        Lyric = lyric
                    };
                    lyricsFile.AddLyric(lyricLine);
                    buffer.Clear();
                }
            }
            return lyricsFile;
        }
        //把秒转换为时分秒格式
        public TimeSpan ConvertTicksToFormattedTime(int tick)
        {
            var secs = timeSynchronizer.GetActualSecsFromTicks(tick);
            var time = TimeSpan.FromSeconds(secs);
            return time;
        }
    }
}
