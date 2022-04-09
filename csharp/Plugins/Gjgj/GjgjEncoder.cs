using System;
using System.Collections.Generic;
using OpenSvip.Model;
using Gjgj.Model;
using OpenSvip.Library;
using OpenSvip.Framework;

namespace Plugin.Gjgj
{
    public class GjgjEncoder
    {
        private Project xsProject;
        
        private GjgjSupportedPinyin gjgjSupportedPinyin = new GjgjSupportedPinyin();
        
        private GjProject gjProject;
        
        private TimeSynchronizer timeSynchronizer;

        private bool isUnsupportedPinyinExist = false;

        private List<string> unsupportedPinyinList = new List<string>();

        public GjProject EncodeProject(Project project)
        {
            xsProject = project;
            timeSynchronizer = new TimeSynchronizer(xsProject.SongTempoList);
            gjProject = new GjProject
            {
                gjgjVersion = 2,
                ProjectSetting = EncodeProjectSetting(),
                TempoMap = EncodeTempoMap()
            };
            EncodeTracks();
            return gjProject;
        }

        private void EncodeTracks()
        {
            int noteID = 1;
            int trackID = 1;
            gjProject.SingingTrackList = new List<GjSingingTrack>(xsProject.TrackList.Count);
            gjProject.InstrumentalTrackList = new List<GjInstrumentalTrack>();
            int trackIndex = 0;
            foreach (var track in xsProject.TrackList)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        gjProject.SingingTrackList.Add(EncodeSingingTrack(ref noteID, trackID, singingTrack));
                        trackID++;
                        break;
                    case InstrumentalTrack instrumentalTrack:
                        gjProject.InstrumentalTrackList.Add(EncodeInstrumentalTrack(trackID, instrumentalTrack));
                        trackID++;
                        break;
                    default:
                        break;
                }
                trackIndex++;
            }
            if (isUnsupportedPinyinExist)
            {
                string unsupportedPinyin = string.Join(",", unsupportedPinyinList);
                Warnings.AddWarning("当前工程文件有歌叽歌叽不支持的拼音，将不会对其进行转换。不支持的拼音：" + unsupportedPinyin);
            }
            gjProject.MIDITrackList = EncodeMIDITrackList();
        }

        private GjInstrumentalTrack EncodeInstrumentalTrack(int trackID, InstrumentalTrack instrumentalTrack)
        {
            GjTrackVolume gjTrackVolume = new GjTrackVolume
            {
                Volume = 1.0f,
                LeftVolume = 1.0f,
                RightVolume = 1.0f,
                Mute = instrumentalTrack.Mute
            };
            GjInstrumentalTrack gjInstrumentalTracksItem = new GjInstrumentalTrack
            {
                TrackID = Convert.ToString(trackID),
                Path = instrumentalTrack.AudioFilePath,
                Offset = 0,//暂不转换伴奏位置偏移
                EQProgram = "",
                SortIndex = 0,
                TrackVolume = gjTrackVolume
            };
            return gjInstrumentalTracksItem;
        }

        private GjSingingTrack EncodeSingingTrack(ref int noteID, int trackID, SingingTrack singingTrack)
        {
            GjSingingTrack gjSingingTracksItem = new GjSingingTrack
            {
                TrackID = Convert.ToString(trackID),
                Type = 0,
                Name = "513singer",//扇宝
                SortIndex = 0,
                NoteList = EncodeNoteList(noteID, singingTrack),
                VolumeParam = EncodeVolumeParam(singingTrack),
                PitchParam = EncodePitchParam(singingTrack),
                Keyboard = EncodeKeyboard(),
                TrackVolume = EncodeTrackVolume(singingTrack),
                EQProgram = "无"
            };
            return gjSingingTracksItem;
        }

        private List<GjMIDITrack> EncodeMIDITrackList()
        {
            return new List<GjMIDITrack>();
        }

        private List<GjNote> EncodeNoteList(int noteID, SingingTrack singingTrack)
        {
            List<GjNote> gjNoteListItems = new List<GjNote>();
            foreach (var note in singingTrack.NoteList)
            {
                gjNoteListItems.Add(EncodeNote(noteID, note));
                noteID++;
            }
            return gjNoteListItems;
        }

        private GjNote EncodeNote(int noteID, Note note)
        {
            GjNote gjNoteListItem = new GjNote
            {
                NoteID = noteID,
                Lyric = note.Lyric,
                Pinyin = GetNotePinyin(note.Pronunciation),
                StartTick = GetNoteStartTick(note.StartPos),
                Duration = note.Length,
                KeyNumber = note.KeyNumber,
                PhonePreTime = GetNotePhonePreTime(note),
                PhonePostTime = GetNotePhonePostTime(note),
                Style = GetNoteStyle(note.HeadTag),
                Velocity = 127
            };
            return gjNoteListItem;
        }

        private GjKeyboard EncodeKeyboard()
        {
            GjKeyboard gjKeyboard = new GjKeyboard
            {
                KeyMode = 1,
                KeyType = 0
            };
            return gjKeyboard;
        }
        
        private GjTrackVolume EncodeTrackVolume(SingingTrack singingTrack)
        {
            GjTrackVolume gjTrackVolume = new GjTrackVolume
            {
                Volume = 1.0f,
                LeftVolume = 1.0f,
                RightVolume = 1.0f,
                Mute = singingTrack.Mute
            };
            return gjTrackVolume;
        }

        private List<GjVolumeParamPoint> EncodeVolumeParam(SingingTrack singingTrack)
        {
            List<GjVolumeParamPoint> gjVolumeParam = new List<GjVolumeParamPoint>();
            try
            {
                List<int> timeBuffer = new List<int>();
                List<double> valueBuffer = new List<double>();
                int time;
                double valueOrigin;
                double value;
                int lastTime = 0;

                for (int index = 1; index < singingTrack.EditedParams.Volume.PointList.Count - 1; index++)
                {
                    time = GetVolumeParamPointTime(index, singingTrack);
                    valueOrigin = GetOriginalVolumeParamPointValue(index, singingTrack);
                    value = GetVolumeParamPointValue(valueOrigin);

                    if (lastTime != time)
                    {
                        if (valueOrigin != 0)
                        {
                            timeBuffer.Add(time);
                            valueBuffer.Add(value);
                        }
                        else
                        {
                            if (timeBuffer.Count == 0 || valueBuffer.Count == 0)
                            {

                            }
                            else
                            {
                                for (int bufferIndex = 0; bufferIndex < timeBuffer.Count; bufferIndex += 5)
                                {
                                    gjVolumeParam.Add(EncodeVolumeParamPoint(timeBuffer[bufferIndex], valueBuffer[bufferIndex]));
                                }
                                gjVolumeParam.Add(EncodeVolumeParamPoint(timeBuffer[0] - 5, 1.0));//左间断点
                                gjVolumeParam.Add(EncodeVolumeParamPoint(timeBuffer[timeBuffer.Count - 1] + 5, 1.0));//右间断点
                                timeBuffer.Clear();
                                valueBuffer.Clear();
                            }
                        }
                    }
                    lastTime = time;
                }
            }
            catch (Exception)
            {

            }
            return gjVolumeParam;
        }

        private int GetVolumeParamPointTime(int index, SingingTrack singingTrack)
        {
            return singingTrack.EditedParams.Volume.PointList[index].Item1;
        }

        private double GetOriginalVolumeParamPointValue(int index, SingingTrack singingTrack)
        {
            return singingTrack.EditedParams.Volume.PointList[index].Item2;
        }

        private GjVolumeParamPoint EncodeVolumeParamPoint(double time, double value)
        {
            GjVolumeParamPoint gjVolumeParamPoint = new GjVolumeParamPoint
            {
                Time = time,
                Value = value
            };
            return gjVolumeParamPoint;
        }

        private double GetVolumeParamPointValue(double volume)
        {
            return (volume + 1000.0) / 1000.0;
        }

        private GjPitchParam EncodePitchParam(SingingTrack singingTrack)
        {
            try
            {
                List<double> timeBuffer = new List<double>();
                List<double> valueBuffer = new List<double>();
                int lastTimeOrigin = -100;
                int timeOrigin;
                int valueOrigin;
                List<GjPitchParamPoint> pitchParamPointList = new List<GjPitchParamPoint>();
                List<GjModifyRange> modifyRangeList = new List<GjModifyRange>();
                for (int index = 0; index < singingTrack.EditedParams.Pitch.PointList.Count; index++)
                {
                    timeOrigin = GetOriginalPitchParamPointTime(index, singingTrack);
                    valueOrigin = GetOriginalPitchParamPointValue(index, singingTrack);

                    if (lastTimeOrigin != timeOrigin)
                    {
                        if (valueOrigin != -100)
                        {
                            timeBuffer.Add(GetPitchParamPointTime(timeOrigin));
                            valueBuffer.Add(GetPitchParamPointValue(valueOrigin));
                        }
                        else
                        {
                            if (timeBuffer.Count == 0 || valueBuffer.Count == 0)
                            {

                            }
                            else
                            {
                                for (int bufferIndex = 0; bufferIndex < timeBuffer.Count; bufferIndex++)
                                {
                                    pitchParamPointList.Add(EncodePitchParamPoint(timeBuffer[bufferIndex], valueBuffer[bufferIndex]));
                                }
                                modifyRangeList.Add(EncodeModifyRange(timeBuffer[0], timeBuffer[valueBuffer.Count - 1]));
                                timeBuffer.Clear();
                                valueBuffer.Clear();
                            }
                        }
                    }
                    lastTimeOrigin = timeOrigin;
                }
                GjPitchParam gjPitchParam = new GjPitchParam
                {
                    PitchParamPointList = pitchParamPointList,
                    ModifyRangeList = modifyRangeList
                };
                return gjPitchParam;
            }
            catch (Exception)
            {
                return new GjPitchParam();
            }
        }

        private int GetOriginalPitchParamPointTime(int index, SingingTrack singingTrack)
        {
            return singingTrack.EditedParams.Pitch.PointList[index].Item1;
        }

        private int GetOriginalPitchParamPointValue(int index, SingingTrack singingTrack)
        {
            return singingTrack.EditedParams.Pitch.PointList[index].Item2;
        }

        private GjPitchParamPoint EncodePitchParamPoint(double time, double value)
        {
            GjPitchParamPoint gjPitchParamPoint = new GjPitchParamPoint
            {
                Time = time,
                Value = value
            };
            return gjPitchParamPoint;
        }

        private GjModifyRange EncodeModifyRange(double left, double right)
        {
            GjModifyRange gjModifyRange = new GjModifyRange
            {
                Left = left,
                Right = right
            };
            return gjModifyRange;
        }

        private double GetPitchParamPointTime(double origin)
        {
            return origin / 5.0;
        }

        private double GetPitchParamPointValue(double origin)
        {
            return ToneToY((double)((origin) / 100.0));
        }

        private GjTempoMap EncodeTempoMap()
        {
            GjTempoMap gjTempoMap = new GjTempoMap
            {
                TicksPerQuarterNote = 480,
                TempoList = EncodeTempoList(),
                TimeSignatureList = EncodeTimeSignatureList()
            };
            return gjTempoMap;
        }

        private List<GjTempo> EncodeTempoList()
        {
            List<GjTempo> gjTempoList = new List<GjTempo>();
            for (int index = 0; index < xsProject.SongTempoList.Count; index++)
            {
                gjTempoList.Add(EncodeTempo(index));
            }
            return gjTempoList;
        }

        private GjTempo EncodeTempo(int index)
        {
            GjTempo gjTempo = new GjTempo
            {
                Time = xsProject.SongTempoList[index].Position,
                MicrosecondsPerQuarterNote = GetMicrosecondsPerQuarterNote(index)
            };
            return gjTempo;
        }

        private List<GjTimeSignature> EncodeTimeSignatureList()
        {
            List<GjTimeSignature> gjTimeSignatureList = new List<GjTimeSignature>(xsProject.TimeSignatureList.Count);
            int sumOfTime = 0;
            for (int index = 0; index < xsProject.TimeSignatureList.Count; index++)
            {

                gjTimeSignatureList.Add(EncodeTimeSignature(sumOfTime, index));
            }
            return gjTimeSignatureList;
        }

        private GjTimeSignature EncodeTimeSignature(int sumOfTime, int index)
        {
            GjTimeSignature gjTimeSignatureItem = new GjTimeSignature
            {
                Time = GetTimeSignatureTime(sumOfTime, index),
                Numerator = GetNumerator(index),
                Denominator = GetDenominator(index)
            };
            return gjTimeSignatureItem;
        }
        
        private int GetTimeSignatureTime(int sumOfTime, int index)
        {
            int time;
            if (index == 0)
            {
                time = GetBarIndex(0) * 1920 * GetNumerator(index) / GetDenominator(index);
            }
            else
            {
                sumOfTime += (GetBarIndex(index) - GetBarIndex(index - 1)) * 1920 * GetNumerator(index - 1) / GetDenominator(index - 1);
                time = sumOfTime;
            }
            return time;
        }
        
        private int GetBarIndex(int index)
        {
            return xsProject.TimeSignatureList[index].BarIndex;
        }

        private int GetNumerator(int index)
        {
            return xsProject.TimeSignatureList[index].Numerator;
        }

        private int GetDenominator(int index)
        {
            return xsProject.TimeSignatureList[index].Denominator;
        }

        private int GetMicrosecondsPerQuarterNote(int index)
        {
            return (int)(60.0 / xsProject.SongTempoList[index].BPM * 1000000.0);
        }

        private GjProjectSetting EncodeProjectSetting()
        {
            GjProjectSetting gjProjectSetting = new GjProjectSetting
            {
                No1KeyName = "C",
                EQAfterMix = "",
                ProjectType = 0,
                Denominator = 4,
                SynMode = 0
            };
            return gjProjectSetting;
        }

        private double ToneToY(double tone)
        {
            return (127 - tone + 0.5) * 18.0;
        }

        private int GetNoteStyle(string origin)
        {
            switch (origin)
            {
                case null:
                    return 0;
                case "V":
                    return 1;
                case "0":
                    return 2;
                default:
                    return 0;
            }
        }

        private int GetNoteStartTick(int origin)
        {
            return origin + 1920 * GetNumerator(0) / GetDenominator(0);
        }

        private string GetNotePinyin(string origin)
        {
            if (origin == null)
            {
                return "";
            }
            else
            {
                string pinyin = origin;
                if (pinyin != "" && !gjgjSupportedPinyin.IsGjSupportedPinyin(pinyin))
                {
                    isUnsupportedPinyinExist = true;
                    unsupportedPinyinList.Add(pinyin);
                    pinyin = "";//过滤不支持的拼音
                }
                return pinyin;
            }
        }
        
        private double GetNotePhonePreTime(Note note)
        {
            double phonePreTime = 0.0;
            try
            {
                if (note.EditedPhones.HeadLengthInSecs != -1.0)
                {
                    int noteStartPositionInTicks = note.StartPos + (1920 * GetNumerator(0) / GetDenominator(0));
                    double noteStartPositionInSeconds = timeSynchronizer.GetActualSecsFromTicks(noteStartPositionInTicks);
                    double phoneHeadPositionInSeconds = noteStartPositionInSeconds - note.EditedPhones.HeadLengthInSecs;
                    double phoneHeadPositionInTicks = timeSynchronizer.GetActualTicksFromSecs(phoneHeadPositionInSeconds);
                    double difference = noteStartPositionInTicks - phoneHeadPositionInTicks;
                    phonePreTime = -difference * 1000.0 / 480.0;
                }
            }
            catch (Exception)
            {

            }
            return phonePreTime; 
        }
        
        private double GetNotePhonePostTime(Note note)
        {
            double phonePostTime = 0.0;
            try
            {
                if (note.EditedPhones.MidRatioOverTail != -1.0)
                {
                    double noteLength = note.Length;
                    double ratio = note.EditedPhones.MidRatioOverTail;
                    phonePostTime = -(noteLength / (1.0 + ratio)) * 1000.0 / 480.0;
                }
            }
            catch (Exception)
            {
                
            }
            return phonePostTime;
        }
    }
}