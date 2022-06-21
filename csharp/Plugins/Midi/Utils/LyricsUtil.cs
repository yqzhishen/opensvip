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
        
        public static void ImportLyricsFromTrackChunk(TrackChunk trackChunk, List<Note> noteList)
        {
            using (var objectsManager = new TimedObjectsManager<TimedEvent>(trackChunk.Events))
            {
                var events = objectsManager.Objects;
                foreach (var note in noteList)
                {
                    try
                    {
                        string lyric = events.Where(e => e.Event is LyricEvent && e.Time == note.StartPos).Select(e => ((LyricEvent)e.Event).Text).FirstOrDefault();
                        if (Regex.IsMatch(lyric, @"[a-zA-Z]"))
                        {
                            note.Lyric = "啊";
                            note.Pronunciation = lyric;
                        }
                        else
                        {
                            note.Lyric = lyric;
                        }
                    }
                    catch
                    {
                        note.Lyric = "啊";
                    }
                }
            }
        }
    }
}
