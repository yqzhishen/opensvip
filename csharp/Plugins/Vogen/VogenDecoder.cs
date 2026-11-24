using FlutyDeer.VogenPlugin.Model;
using FlutyDeer.VogenPlugin.Options;
using OpenSvip.Model;
using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.VogenPlugin
{
    public class VogenDecoder
    {
        public MergePhraseOption MergePhrase { get; set; }

        public Project DecodeProject(VogenProject originalProject)
        {
            var vogProject = originalProject;
            string strTimeSignature = vogProject.TimeSignature;
            string[] strTimeSignatureArray = strTimeSignature.Split('/');
            TimeSignature timeSignature = new TimeSignature
            {
                Numerator = int.Parse(strTimeSignatureArray[0]),
                Denominator = int.Parse(strTimeSignatureArray[1])
            };
            SongTempo tempo = new SongTempo
            {
                Position = 0,
                BPM = vogProject.BPM
            };

            Project osProject = new Project
            {
                Version = "SVIP7.0.0",
                SongTempoList = new List<SongTempo> { tempo },
                TimeSignatureList = new List<TimeSignature> { timeSignature }
            };
            osProject.TrackList = DecodeTrackList(vogProject);
            return osProject;
        }

        /// <summary>
        /// 转换演唱轨和伴奏轨。
        /// </summary>
        /// <returns></returns>
        private List<Track> DecodeTrackList(VogenProject vogProject)
        {
            return GroupPhrase(vogProject.PhraseList)
                .Select(PhrasesToTrack)
                .ToList();
        }

        private int EndPos(VogPhrase phrase)
        {
            if (phrase.NoteList.Count == 0) {
                return -1;
            }
            var lastNote = phrase.NoteList.Last();
            return lastNote.StartPosition + lastNote.Duration;
        }

        private List<List<VogPhrase>> GroupPhrasesWithinSinger(IEnumerable<VogPhrase> naiveGroup)
        {
            switch (MergePhrase)
            {
                case MergePhraseOption.All:
                    return new List<List<VogPhrase>> { naiveGroup.ToList() };
                case MergePhraseOption.Auto:
                    var groups = new List<List<VogPhrase>>();
                    foreach (var phrase in naiveGroup) {
                        var startPos = phrase.NoteList[0].StartPosition;
                        var destination = groups
                            .FirstOrDefault(group => EndPos(group.Last()) < startPos);
                        if (destination == null) { 
                            destination = new List<VogPhrase>();
                            groups.Add(destination);
                        }
                        destination.Add(phrase);
                    }
                    return groups;
                default:
                    return naiveGroup
                        .Select(phrase => new List<VogPhrase> { phrase })
                        .ToList();
            }
        }

        private List<List<VogPhrase>> GroupPhrase(List<VogPhrase> phraseList)
        {
            return phraseList
                .GroupBy(p => p.SingerName + " " + p.RomScheme)
                .SelectMany(GroupPhrasesWithinSinger)
                .ToList();
        }

        private Track PhrasesToTrack(List<VogPhrase> phrases)
        {
            return new SingingTrack
            {
                Title = phrases.FirstOrDefault()?.SingerName ?? "",
                Mute = false,
                Solo = false,
                Volume = 0.7,
                Pan = 0.0,
                AISingerName = GetDefaultAISingerName(),
                ReverbPreset = GetDefaultReverbPreset(),
                NoteList = phrases.SelectMany(ph => ph.NoteList.Select(DecodeNote)).ToList(),
            };
        }

        /// <summary>
        /// 转换音符。
        /// </summary>
        /// <param name="singingTrackIndex">演唱轨索引。</param>
        /// <param name="noteIndex">音符索引。</param>
        /// <returns></returns>
        private Note DecodeNote(VogNote vogNote)
        {
            Note note = new Note
            {
                StartPos = vogNote.StartPosition,
                Length = vogNote.Duration,
                KeyNumber = vogNote.KeyNumber,
                Lyric = vogNote.Lyric,
                Pronunciation = vogNote.Pronunciation,
            };
            return note;
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
    }
}
