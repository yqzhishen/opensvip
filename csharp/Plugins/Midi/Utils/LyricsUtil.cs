using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Note = OpenSvip.Model.Note;

namespace FlutyDeer.MidiPlugin.Utils
{
    public static class LyricsUtil
    {
        public static List<string> SymbolToRemoveList()
        {
            string[] unsupportedSymbolArray = { ",", ".", "?", "!", "，", "。", "？", "！" };
            return unsupportedSymbolArray.ToList();
        }

        public static string GetSymbolRemovedLyric(string lyric)
        {
            if (lyric.Length > 1)
            {
                foreach (var symbol in SymbolToRemoveList())
                {
                    lyric = lyric.Replace(symbol, "");
                }
            }
            return lyric;
        }
        
        public static void ImportLyricsFromTrackChunk(TrackChunk trackChunk, List<Note> noteList, short PPQ)
        {
            using (var objectsManager = new TimedObjectsManager<TimedEvent>(trackChunk.Events))
            {
                var events = objectsManager.Objects;
                var lyricsDict = events
                    .Where(e => e.Event is LyricEvent)
                    .ToDictionary(e => e.Time * 480 / PPQ, e => ((LyricEvent)e.Event).Text);
                foreach (var note in noteList)
                {
                    if (lyricsDict.ContainsKey(note.StartPos))
                    {
                        note.Lyric = lyricsDict[note.StartPos];
                    }
                }
            }
        }
    }
}
