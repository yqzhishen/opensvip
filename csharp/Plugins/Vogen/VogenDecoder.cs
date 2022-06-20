using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlutyDeer.VogenPlugin.Model;

namespace FlutyDeer.VogenPlugin
{
    public class VogenDecoder
    {
        private VogenProject vogProject;

        public Project DecodeProject(VogenProject originalProject)
        {
            vogProject = originalProject;
            Project osProject = new Project
            {
                Version = "SVIP7.0.0",
                SongTempoList = DecodeSongTempoList(),
                TimeSignatureList = DecodeTimeSignatureList()
            };
            osProject.TrackList = DecodeTrackList(osProject);
            return osProject;
        }

        /// <summary>
        /// 转换演唱轨和伴奏轨。
        /// </summary>
        /// <returns></returns>
        private List<Track> DecodeTrackList(Project project)
        {
            List<Track> trackList = new List<Track>();
            trackList.AddRange(DecodeSingingTracks(project));
            return trackList;
        }

        private List<Track> DecodeSingingTracks(Project project)
        {
            List<Track> singingTrackList = new List<Track>();
            for (int index = 0; index < vogProject.TrackList.Count; index++)
            {
                Track track = new SingingTrack
                {
                    Title = vogProject.TrackList[index].SingerName,
                    Mute = false,
                    Solo = false,
                    Volume = 0.7,
                    Pan = 0.0,
                    AISingerName = GetDefaultAISingerName(),
                    ReverbPreset = GetDefaultReverbPreset(),
                    NoteList = DecodeNoteList(index, project)
                };
                singingTrackList.Add(track);
            }
            return singingTrackList;
        }

        /// <summary>
        /// 转换音符列表。
        /// </summary>
        /// <param name="singingTrackIndex">演唱轨索引。</param>
        /// <param name="project">OpenSvip工程。</param>
        /// <returns></returns>
        private List<Note> DecodeNoteList(int singingTrackIndex, Project project)
        {
            List<Note> noteList = new List<Note>();
            for (int noteIndex = 0; noteIndex < vogProject.TrackList[singingTrackIndex].NoteList.Count; noteIndex++)
            {
                noteList.Add(DecodeNote(singingTrackIndex, noteIndex, project));
            }
            return noteList;
        }

        /// <summary>
        /// 转换音符。
        /// </summary>
        /// <param name="singingTrackIndex">演唱轨索引。</param>
        /// <param name="noteIndex">音符索引。</param>
        /// <returns></returns>
        private Note DecodeNote(int singingTrackIndex, int noteIndex, Project project)
        {
            Note note = new Note
            {
                StartPos = DecodeNoteStartPosition(singingTrackIndex, noteIndex, project),
                Length = vogProject.TrackList[singingTrackIndex].NoteList[noteIndex].Duration,
                KeyNumber = vogProject.TrackList[singingTrackIndex].NoteList[noteIndex].KeyNumber,
                Lyric = vogProject.TrackList[singingTrackIndex].NoteList[noteIndex].Lyric,
                Pronunciation = DecodePronunciation(singingTrackIndex, noteIndex),
            };
            return note;
        }

        /// <summary>
        /// 转换音符起始位置。
        /// </summary>
        /// <param name="singingTrackIndex">演唱轨索引。</param>
        /// <param name="noteIndex">音符索引。</param>
        /// <param name="project">OpenSvip工程。</param>
        /// <returns></returns>
        private int DecodeNoteStartPosition(int singingTrackIndex, int noteIndex, Project project)
        {
            return vogProject.TrackList[singingTrackIndex].NoteList[noteIndex].StartPosition;
        }

        /// <summary>
        /// 转换音符读音。
        /// </summary>
        /// <param name="singingTrackIndex"></param>
        /// <param name="noteIndex"></param>
        /// <returns></returns>
        private string DecodePronunciation(int singingTrackIndex, int noteIndex)
        {
            string pronunciation = vogProject.TrackList[singingTrackIndex].NoteList[noteIndex].Pronunciation;
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
            string strTimeSignature = vogProject.TimeSignature;
            string[] strTimeSignatureArray = strTimeSignature.Split('/');
            TimeSignature timeSignature = new TimeSignature
            {
                Numerator = int.Parse(strTimeSignatureArray[0]),
                Denominator = int.Parse(strTimeSignatureArray[1])
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
                BPM = vogProject.BPM
            };
            return songTempo;
        }
        
    }
}
