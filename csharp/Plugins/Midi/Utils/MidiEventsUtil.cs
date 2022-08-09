using FlutyDeer.GjgjPlugin.Utils;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NPinyin;
using OpenSvip.Model;
using System.Collections.Generic;
using Note = OpenSvip.Model.Note;

namespace FlutyDeer.MidiPlugin.Utils
{
    public class MidiEventsUtil
    {

        public int SemivowelPreShift { get; set; }
        
        public bool IsExportLyrics { get; set; }

        public bool IsUseCompatibleLyric { get; set; }

        public bool IsUseLegacyPinyin { get; set; }

        public bool IsRemoveSymbols { get; set; }

        public bool IsConstantTempo { get; set; }
        
        public int Transpose { get; set; }

        /// <summary>
        /// 将曲速转换为每四分音符的微秒数。
        /// </summary>
        /// <param name="BPM">曲速。</param>
        /// <returns>每四分音符的微秒数。</returns>
        public long BPMToMicrosecondsPerQuarterNote(float BPM)
        {
            return (long)(60000000.0 / BPM);
        }

        /// <summary>
        /// 转换演唱轨。
        /// </summary>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns>含有音符事件数组的 Track Chunk。</returns>
        public TrackChunk SingingTrackToMidiTrackChunk(SingingTrack singingTrack)
        {
            PreShiftUtil.PreShiftSemivowelNotes(singingTrack.NoteList, SemivowelPreShift);
            List<MidiEvent> midiEventList = new List<MidiEvent>();
            midiEventList.Add(new SequenceTrackNameEvent(singingTrack.Title));//写入轨道名称
            TrackChunk trackChunk = new TrackChunk(midiEventList.ToArray());
            List<string> lyricList = new List<string>();
            lyricList.Clear();
            foreach (var note in singingTrack.NoteList)
            {
                lyricList.Add(note.Lyric);
            }
            PinyinAndLyricUtil.ClearAllPinyin();
            PinyinAndLyricUtil.AddPinyin(lyricList);
            using (var objectsManager = new TimedObjectsManager<TimedEvent>(trackChunk.Events))
            {
                var events = objectsManager.Objects;
                int index = 0;
                foreach (var note in singingTrack.NoteList)
                {
                    var start = note.StartPos;
                    var end = note.StartPos + note.Length;
                    if (IsConstantTempo)
                    {

                    }
                    if(IsExportLyrics)
                    {
                        events.Add(new TimedEvent(new LyricEvent(GetLyric(note, index)), note.StartPos));
                    }
                    events.Add(new TimedEvent(new NoteOnEvent(GetTransposedKeyNumber(note), (SevenBitNumber)45), note.StartPos));
                    events.Add(new TimedEvent(new NoteOffEvent(GetTransposedKeyNumber(note), (SevenBitNumber)0), note.StartPos + note.Length));
                    index++;
                }
            }
            return trackChunk;
        }

        /// <summary>
        /// 根据输出选项转换歌词。
        /// </summary>
        /// <param name="note">音符。</param>
        /// <returns></returns>
        private string GetLyric(Note note, int index)
        {
            string lyric;
            if (note.Pronunciation != null)
            {
                lyric = note.Pronunciation;
            }
            else
            {
                lyric = note.Lyric;
                if (IsRemoveSymbols)
                {
                    lyric = LyricsUtil.GetSymbolRemovedLyric(lyric);
                }
                if (IsUseCompatibleLyric)
                {
                    lyric = PinyinAndLyricUtil.GetNotePinyin(lyric, IsUseLegacyPinyin, index);
                }
            }
            return lyric;
        }

        /// <summary>
        /// 获取移调后的音高。
        /// </summary>
        /// <param name="note">音符。</param>
        /// <returns>移调后的音高。</returns>
        private SevenBitNumber GetTransposedKeyNumber(Note note)
        {
            int transposedKeyNumber = note.KeyNumber + Transpose;
            if (transposedKeyNumber < 0)
            {
                transposedKeyNumber = 0;
            }
            else if (transposedKeyNumber > 127)
            {
                transposedKeyNumber = 127;
            }
            return (SevenBitNumber)transposedKeyNumber;
        }
    }
}