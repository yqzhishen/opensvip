using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Note = OpenSvip.Model.Note;
using TimeSignature = OpenSvip.Model.TimeSignature;
using System.Text.RegularExpressions;

namespace FlutyDeer.MidiPlugin
{
    public class MidiDecoder
    {
        /// <summary>
        /// 歌词文本编码。
        /// </summary>
        public LyricEncodings LyricEncoding { get; set; }
        private MidiFile midiFile;
        public Project DecodeMidiFile(string path)
        {
            var osProject = new Project();
            ReadingSettings readingSettings = new ReadingSettings();
            readingSettings.TextEncoding = EncodingUtil.GetEncoding(LyricEncoding);
            midiFile = MidiFile.Read(path, readingSettings);
            IEnumerable<TrackChunk> midiChunk = midiFile.GetTrackChunks();
            bool isTempoMapTrack = true;
            List<Track> singingTrackList = new List<Track>();
            foreach (var chunkItem in midiChunk)
            {
                if (isTempoMapTrack)//解析曲速轨
                {
                    IEnumerable<MidiEvent> midiEvents = chunkItem.Events;
                    if (midiEvents.Count() > 0)
                    {
                        try
                        {
                            long previousEventTime = 0;
                            foreach (var midiEvent in midiEvents)
                            {
                                if (midiEvent.EventType == MidiEventType.SetTempo)
                                {
                                    string setTempoEventStr = midiEvent.ToString();
                                    float microsecondsPerQuarterNote = float.Parse(setTempoEventStr.Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[1]);
                                    var songTempo = new SongTempo
                                    {
                                        Position = (int)(previousEventTime),
                                        BPM = (float)(60.0 / microsecondsPerQuarterNote * 1000000.0)
                                    };
                                    osProject.SongTempoList.Add(songTempo);
                                }
                                previousEventTime += midiEvent.DeltaTime;
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    isTempoMapTrack = false;
                }
                else//解析其他轨
                {
                    IEnumerable<MidiEvent> midiEvents = chunkItem.Events;
                    int previousEventTime = 0;
                    List<Note> noteList = new List<Note>();
                    string tempLyric = "啊";
                    string tempPronunciation = null;
                    int tempStratPosition = 0;
                    int tempDuration = 0;
                    int tempKeyNumber = 0;
                    foreach (var midiEvent in midiEvents)
                    {
                        switch (midiEvent.EventType)
                        {
                            case MidiEventType.Lyric:
                                string lyricEventStr = midiEvent.ToString();
                                string lyric = lyricEventStr.Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[1];
                                if (Regex.IsMatch(lyric, @"[a-zA-Z]"))
                                {
                                    tempLyric = "啊";
                                    tempPronunciation = lyric;
                                }
                                else
                                {
                                    tempLyric = lyric;
                                }
                                break;
                            case MidiEventType.NoteOn:
                                string noteOnEventStr = midiEvent.ToString();
                                string noteOnStr = noteOnEventStr.Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[1];
                                string[] noteOnStrArray = noteOnStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                tempKeyNumber = int.Parse(noteOnStrArray[0]);
                                tempStratPosition = previousEventTime;
                                break;
                            case MidiEventType.NoteOff:
                                tempDuration = (int)midiEvent.DeltaTime;
                                Note note = new Note
                                {
                                    Lyric = tempLyric,
                                    Pronunciation = tempPronunciation,
                                    StartPos = tempStratPosition,
                                    Length = tempDuration,
                                    KeyNumber = tempKeyNumber
                                };
                                tempPronunciation = null;
                                noteList.Add(note);
                                break;
                        }
                        previousEventTime += (int)midiEvent.DeltaTime;
                    }
                    singingTrackList.Add(new SingingTrack
                    {
                        Title = "演唱轨",
                        Mute = false,
                        Solo = false,
                        Volume = 0.7,
                        Pan = 0.0,
                        AISingerName = "陈水若",
                        ReverbPreset = "干声",
                        NoteList = noteList
                    });
                }
            }
            osProject.TrackList.AddRange(singingTrackList);
            osProject.TimeSignatureList.Add(new TimeSignature
            {
                BarIndex = 0,
                Numerator = 4,
                Denominator = 4
            });
            return osProject;
        }
    }
}
