using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Y77.Model;

namespace Plugin.Y77
{
    public class Y77Decoder
    {
        private Y77Project y77Project;

        public Project DecodeProject(Y77Project originalProject)
        {
            y77Project = originalProject;
            Project osProject = new Project
            {
                Version = "SVIP7.0.0",
                SongTempoList = DecodeSongTempoList(),
                TimeSignatureList = DecodeTimeSignatureList()
            };
            osProject.TrackList = DecodeTrackList();
            return osProject;
        }

        /// <summary>
        /// 转换演唱轨和伴奏轨。
        /// </summary>
        /// <returns></returns>
        private List<Track> DecodeTrackList()
        {
            List<Track> trackList = new List<Track>();
            trackList.AddRange(DecodeSingingTracks());
            return trackList;
        }

        private List<Track> DecodeSingingTracks()
        {
            List<Track> singingTrackList = new List<Track>();
            Track track = new SingingTrack
            {
                Title = "元七七",
                Mute = false,
                Solo = false,
                Volume = 0.7,
                Pan = 0.0,
                AISingerName = GetDefaultAISingerName(),
                ReverbPreset = GetDefaultReverbPreset(),
                NoteList = DecodeNoteList()
            };
            singingTrackList.Add(track);
            return singingTrackList;
        }

        /// <summary>
        /// 转换音符列表。
        /// </summary>
        /// <param name="project">OpenSvip工程。</param>
        /// <returns></returns>
        private List<Note> DecodeNoteList()
        {
            List<Note> noteList = new List<Note>();
            for (int noteIndex = 0; noteIndex < y77Project.NoteList.Count; noteIndex++)
            {
                noteList.Add(DecodeNote(noteIndex));
            }
            return noteList;
        }

        /// <summary>
        /// 转换音符。
        /// </summary>
        /// <param name="noteIndex">音符索引。</param>
        /// <returns></returns>
        private Note DecodeNote(int noteIndex)
        {
            Note note = new Note
            {
                StartPos = y77Project.NoteList[noteIndex].StartPosition * 30,
                Length = y77Project.NoteList[noteIndex].Length * 30,
                KeyNumber = 88 - y77Project.NoteList[noteIndex].KeyNumber,
                Lyric = y77Project.NoteList[noteIndex].Lyric,
                Pronunciation = DecodePronunciation(noteIndex),
            };
            return note;
        }

        /// <summary>
        /// 转换音符读音。
        /// </summary>
        /// <param name="noteIndex"></param>
        /// <returns></returns>
        private string DecodePronunciation(int noteIndex)
        {
            string pronunciation = y77Project.NoteList[noteIndex].Pinyin;
            foreach (var symbol in SymbolList.SymbolToRemoveList())
            {
                pronunciation = pronunciation.Replace(symbol, "");
            }
            pronunciation.Replace(" ", "");
            return pronunciation;
        }

        /// <summary>
        /// 设置混响类型。
        /// </summary>
        /// <returns></returns>
        private static string GetDefaultReverbPreset()
        {
            return "干声";
        }

        /// <summary>
        /// 设置默认歌手。
        /// </summary>
        /// <returns></returns>
        private static string GetDefaultAISingerName()
        {
            return "陈水若";
        }

        private List<TimeSignature> DecodeTimeSignatureList()
        {
            List<TimeSignature> timeSignatureList = new List<TimeSignature>();
            timeSignatureList.Add(DecodeTimeSignature());
            return timeSignatureList;
        }

        private TimeSignature DecodeTimeSignature()
        {
            TimeSignature timeSignature = new TimeSignature
            {
                Numerator = y77Project.TimeSignatureNumerator,
                Denominator = y77Project.TimeSignatureDenominator
            };
            return timeSignature;
        }

        /// <summary>
        /// 转换曲速列表。
        /// </summary>
        /// <returns></returns>
        private List<SongTempo> DecodeSongTempoList()
        {
            List<SongTempo> songTempoList = new List<SongTempo>();
            songTempoList.Add(DecodeSongTempo());
            return songTempoList;
        }

        /// <summary>
        /// 转换曲速标记。
        /// </summary>
        /// <returns></returns>
        private SongTempo DecodeSongTempo()
        {
            SongTempo songTempo = new SongTempo
            {
                Position = 0,
                BPM = y77Project.BPM
            };
            return songTempo;
        }
    }
}
