using NPinyin;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Y77.Model;

namespace Plugin.Y77
{
    public class Y77Encoder
    {
        private Project osProject;

        private int noteCount = 0;

        public Y77Project EncodeProject(Project project)
        {
            osProject = project;
            Y77Project y77Project = new Y77Project
            {
                BarCount = 100,
                NoteList = EncodeNoteList(),
                NoteCount = noteCount,
                BPM = osProject.SongTempoList[0].BPM,
                TimeSignatureNumerator = GetNumerator(0),
                TimeSignatureDenominator = GetDenominator(0)
            };
            return y77Project;
        }

        private List<Y77Note> EncodeNoteList()
        {
            List<Y77Note> y77NoteList = new List<Y77Note>();
            foreach (var track in osProject.TrackList)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        foreach (var note in singingTrack.NoteList)
                        {
                            y77NoteList.Add(EncodeNote(note));
                        }
                        break;
                    default:
                        break;
                }
                break;
            }
            noteCount = y77NoteList.Count;
            return y77NoteList;
        }

        private Y77Note EncodeNote(Note note)
        {
            Y77Note y77Note = new Y77Note
            {
                Pinyin = GetNotePinyin(note),
                Length = note.Length / 30,
                StartPosition = note.StartPos / 30,
                Lyric = note.Lyric,
                KeyNumber = 88 - note.KeyNumber,
                PitchParam = EncodePitchParam()
            };
            return y77Note;
        }

        /// <summary>
        /// 转换音符的拼音。
        /// </summary>
        /// <returns>将歌词转为拼音。</returns>
        private string GetNotePinyin(Note note)
        {
            string origin = note.Pronunciation;
            if (origin == null)
            {
                string lyric = note.Lyric;
                if (lyric.Length > 1)
                {
                    foreach (var symbol in SymbolList.SymbolToRemoveList())
                    {
                        lyric = lyric.Replace(symbol, "");
                    }
                }
                origin = Pinyin.GetPinyin(lyric);
                return origin;
            }
            else
            {
                return origin;
            }
        }
        
        private List<int> EncodePitchParam()
        {
            List<int> pitchParam = new List<int>();
            for (int i = 0; i < 500; i++)
            {
                pitchParam.Add(50);
            }
            return pitchParam;
        }

        /// <summary>
        /// 返回拍号的分子。
        /// </summary>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        private int GetNumerator(int index)
        {
            return osProject.TimeSignatureList[index].Numerator;
        }

        /// <summary>
        /// 返回拍号的分母。
        /// </summary>
        /// <param name="index">原始拍号的索引。</param>
        /// <returns></returns>
        private int GetDenominator(int index)
        {
            return osProject.TimeSignatureList[index].Denominator;
        }
    }
}
