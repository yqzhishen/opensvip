using FlutyDeer.GjgjPlugin.Model;
using FlutyDeer.GjgjPlugin.Utils;
using OpenSvip.Library;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace FlutyDeer.GjgjPlugin
{
    public class GjgjDecoder
    {
        private GjProject gjProject;

        private TimeSynchronizer timeSynchronizer;

        /// <summary>
        /// 转换为OpenSvip工程。
        /// </summary>
        /// <param name="originalProject">原始的歌叽歌叽工程。</param>
        /// <returns>转换后的OpenSvip</returns>
        public Project DecodeProject(GjProject originalProject)
        {
            gjProject = originalProject;
            var osProject = new Project
            {
                SongTempoList = DecodeSongTempoList(gjProject.TempoMap.TempoList),
                TimeSignatureList = DecodeTimeSignatureList(gjProject.TempoMap.TimeSignatureList)
            };
            timeSynchronizer = new TimeSynchronizer(osProject.SongTempoList);
            osProject.TrackList = DecodeTrackList(osProject);
            return osProject;
        }

        /// <summary>
        /// 转换演唱轨和伴奏轨。
        /// </summary>
        /// <param name="project">OpenSvip工程。</param>
        /// <returns></returns>
        private List<Track> DecodeTrackList(Project project)
        {
            List<Track> trackList = new List<Track>();
            trackList.AddRange(DecodeSingingTracks(project));
            trackList.AddRange(DecodeInstrumentalTracks());
            return trackList;
        }

        /// <summary>
        /// 转换演唱轨。
        /// </summary>
        /// <param name="project">OpenSvip工程。</param>
        /// <returns></returns>
        private List<Track> DecodeSingingTracks(Project project)
        {
            List<Track> singingTrackList = new List<Track>();
            foreach(var gjSingingTrack in gjProject.SingingTrackList)
            {
                Track track = new SingingTrack
                {
                    Title = SingerNameUtil.GetSingerName(gjSingingTrack),
                    Mute = gjSingingTrack.TrackVolume.Mute,
                    Solo = false,
                    Volume = 0.7,
                    Pan = 0.0,
                    AISingerName = "陈水若",
                    ReverbPreset = "干声",
                    NoteList = DecodeNoteList(gjSingingTrack, project),
                    EditedParams = DecodeParams(gjSingingTrack)
                };
                singingTrackList.Add(track);
            }
            return singingTrackList;
        }

        /// <summary>
        /// 转换伴奏轨。
        /// </summary>
        /// <returns></returns>
        private List<Track> DecodeInstrumentalTracks()
        {
            List<Track> instrumentalTrackList = new List<Track>();
            foreach (var instTrack in gjProject.InstrumentalTrackList)
            {
                Track track = new InstrumentalTrack
                {
                    Title = GetInstrumentalName(instTrack.Path),
                    Mute = instTrack.TrackVolume.Mute,
                    Solo = false,
                    Volume = 0.3,
                    Pan = 0.0,
                    AudioFilePath = instTrack.Path,
                    Offset = DecodeInstOffset(instTrack.Offset),
                };
                instrumentalTrackList.Add(track);
            }
            return instrumentalTrackList;
        }

        private int DecodeInstOffset(int offset)
        {
            return (int)(timeSynchronizer.GetActualTicksFromSecs(offset / 10000000) - 1920 * GetNumerator(0) / GetDenominator(0));
        }

        /// <summary>
        /// 转换音符列表。
        /// </summary>
        /// <param name="project">OpenSvip工程。</param>
        /// <returns></returns>
        private List<Note> DecodeNoteList(GjSingingTrack gjSingingTrack, Project project)
        {
            List<Note> noteList = new List<Note>();
            foreach(var gjNote in gjSingingTrack.NoteList)
            {
                noteList.Add(DecodeNote(gjNote, project));
            }
            return noteList;
        }

        /// <summary>
        /// 转换音符。
        /// </summary>
        /// <param name="project">OpenSvip工程。</param>
        /// <returns></returns>
        private Note DecodeNote(GjNote gjNote, Project project)
        {
            int noteStartPosition = DecodeNoteStartPosition(gjNote, project);
            Note note = new Note
            {
                StartPos = noteStartPosition,
                Length = gjNote.Duration,
                KeyNumber = gjNote.KeyNumber,
                Lyric = gjNote.Lyric,
                Pronunciation = PinyinAndLyricUtil.DecodePronunciation(gjNote),
                EditedPhones = DecodePhones(gjNote, noteStartPosition),
                HeadTag = NoteHeadTagUtil.ToStringTag(gjNote.Style)
            };
            return note;
        }

        /// <summary>
        /// 转换音符起始位置。
        /// </summary>
        /// <param name="project">OpenSvip工程。</param>
        /// <returns></returns>
        private int DecodeNoteStartPosition(GjNote gjNote, Project project)//用了opensvip的拍号列表，是因为gj可能存在拍号列表里面没有拍号的情况
        {
            return gjNote.StartTick - 1920 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator;
        }

        /// <summary>
        /// 转换音素。
        /// </summary>
        /// <returns></returns>
        private Phones DecodePhones(GjNote gjNote, int startPosition)
        {
            int duration = gjNote.Duration;
            double preTime = gjNote.PhonePreTime;
            double postTime = gjNote.PhonePostTime;
            Phones phones = new Phones();
            try
            {
                if (preTime != 0.0)
                {
                    phones.HeadLengthInSecs = DecodeHeadLengthInSecs(startPosition, preTime);
                }
                if (postTime != 0.0)
                {
                    phones.MidRatioOverTail = DecodeMidRatioOverTail(duration, postTime);
                }
            }
            catch (Exception)
            {

            }
            return phones;
        }

        /// <summary>
        /// 转换音素第一根杆子。
        /// </summary>
        /// <param name="startPosition">音符起始位置。</param>
        /// <param name="preTime">原始音素第一根杆子。</param>
        /// <returns></returns>
        private float DecodeHeadLengthInSecs(int startPosition, double preTime)
        {
            int difference;
            float headLengthInSecs;
            difference = startPosition + (int)(preTime * 480.0 * 3.0 / 2000.0);
            if (difference > 0)
            {
                headLengthInSecs = (float)(timeSynchronizer.GetActualSecsFromTicks(startPosition) - timeSynchronizer.GetActualSecsFromTicks(difference));
            }
            else
            {
                headLengthInSecs = -1.0f;
            }
            return headLengthInSecs;
        }

        /// <summary>
        /// 转换音素第二根杆子。
        /// </summary>
        /// <param name="duration">音符长度。</param>
        /// <param name="postTime">原始音素第二根杆子。</param>
        /// <returns></returns>
        private float DecodeMidRatioOverTail(int duration, double postTime)
        {
            float midRatioOverTail;
            double essentialVowelLength;
            double tailVowelLength;
            if (postTime != 0)
            {
                essentialVowelLength = duration * (2000.0 / 3.0) / 480 + postTime;
                tailVowelLength = -postTime;
                midRatioOverTail = (float)(essentialVowelLength / tailVowelLength);
            }
            else
            {
                midRatioOverTail = -1.0f;
            }
            return midRatioOverTail;
        }

        /// <summary>
        /// 转换参数。
        /// </summary>
        /// <returns></returns>
        private Params DecodeParams(GjSingingTrack gjSingingTrack)
        {
            Params paramsFromGj = new Params
            {
                Volume = VolumeParamUtil.DecodeVolumeParam(gjSingingTrack.VolumeParam),
                Pitch = PitchParamUtil.DecodePitchParam(gjSingingTrack.PitchParam)
            };
            return paramsFromGj;
        }

        /// <summary>
        /// 转换拍号列表。
        /// </summary>
        /// <returns></returns>
        private List<TimeSignature> DecodeTimeSignatureList(List<GjTimeSignature> gjTimeSignatureList)
        {
            List<TimeSignature> timeSignatureList = new List<TimeSignature>();
            if (gjTimeSignatureList.Count == 0)//如果拍号只有4/4，gjgj不存
            {
                timeSignatureList.Add(GetInitialTimeSignature());
            }
            else
            {
                if (gjTimeSignatureList[0].Time != 0)//如果存的第一个拍号不在0处，说明0处的拍号是4/4
                {
                    timeSignatureList.Add(GetInitialTimeSignature());

                    int sumOfTime = 0;
                    for (int index = 0; index < gjTimeSignatureList.Count; index++)
                    {
                        TimeSignature timeSignature = new TimeSignature();
                        if (index == 0)
                        {
                            sumOfTime += gjTimeSignatureList[0].Time / 1920;
                            timeSignature.BarIndex = sumOfTime;
                        }
                        else
                        {
                            sumOfTime += (GetGjTimeSignatureTime(index) - GetGjTimeSignatureTime(index - 1)) * GetDenominator(index - 1) / 1920 / GetNumerator(index - 1);
                            timeSignature.BarIndex = sumOfTime;
                        }
                        timeSignature.Numerator = GetNumerator(index);
                        timeSignature.Denominator = GetDenominator(index);
                        timeSignatureList.Add(timeSignature);
                    }
                }
                else
                {
                    int sumOfTime = 0;
                    for (int index = 0; index < gjTimeSignatureList.Count; index++)
                    {
                        TimeSignature timeSignature = new TimeSignature();
                        if (index == 0)
                        {
                            timeSignature.BarIndex = 0;
                        }
                        else
                        {
                            sumOfTime += (GetGjTimeSignatureTime(index) - GetGjTimeSignatureTime(index - 1)) * GetDenominator(index - 1) / 1920 / GetNumerator(index - 1);
                            timeSignature.BarIndex = sumOfTime;
                        }
                        timeSignature.Numerator = GetNumerator(index);
                        timeSignature.Denominator = GetDenominator(index);
                        timeSignatureList.Add(timeSignature);
                    }
                }
            }
            return timeSignatureList;
        }

        /// <summary>
        /// 转换曲速列表。
        /// </summary>
        /// <returns></returns>
        private List<SongTempo> DecodeSongTempoList(List<GjTempo> gjTempoList)
        {
            List<SongTempo> songTempoList = new List<SongTempo>();
            foreach (GjTempo gjTempo in gjTempoList)
            {
                songTempoList.Add(TempoUtil.DecodeSongTempo(gjTempo));
            }
            return songTempoList;
        }

        /// <summary>
        /// 返回伴奏文件名作为伴奏轨的标题。
        /// </summary>
        /// <returns></returns>
        private string GetInstrumentalName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);//返回伴奏文件名作为轨道标题
        }

        /// <summary>
        /// 返回一个四四拍的初始拍号标记。
        /// </summary>
        /// <returns></returns>
        private TimeSignature GetInitialTimeSignature()
        {
            TimeSignature defaultTimeSignature = new TimeSignature
            {
                BarIndex = 0,
                Numerator = 4,
                Denominator = 4
            };
            return defaultTimeSignature;
        }

        /// <summary>
        /// 转换拍号标记的时间。
        /// </summary>
        /// <param name="index">拍号标记索引。</param>
        /// <returns></returns>
        private int GetGjTimeSignatureTime(int index)
        {
            return gjProject.TempoMap.TimeSignatureList[index].Time;
        }

        /// <summary>
        /// 返回拍号标记的分母。
        /// </summary>
        /// <param name="index">拍号标记索引。</param>
        /// <returns></returns>
        private int GetDenominator(int index)
        {
            return gjProject.TempoMap.TimeSignatureList[index].Denominator;
        }

        /// <summary>
        /// 返回拍号标记的分子。
        /// </summary>
        /// <param name="index">拍号标记索引。</param>
        /// <returns></returns>
        private int GetNumerator(int index)
        {
            return gjProject.TempoMap.TimeSignatureList[index].Numerator;
        }
    }
}