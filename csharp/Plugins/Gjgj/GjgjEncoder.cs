using System;
using System.Collections.Generic;
using OpenSvip.Model;
using Gjgj.Model;
using OpenSvip.Library;

namespace Plugin.Gjgj
{
    public class GjgjEncoder
    {
        public GjProject EncodeProject(Project project)
        {
            GjProject gjProject = new GjProject();
            SetGjProjectProperties(gjProject);
            EncodeTempo(project, gjProject);
            TimeSynchronizer timeSynchronizer = new TimeSynchronizer(project.SongTempoList);
            EncodeTimeSignature(project, gjProject);
            EncodeTracks(project, gjProject, timeSynchronizer);
            return gjProject;
        }

        private void EncodeTracks(Project project, GjProject gjProject, TimeSynchronizer timeSynchronizer)
        {
            int noteID = 1;
            int trackID = 1;
            gjProject.SingingTracks = new List<GjSingingTracksItem>(project.TrackList.Count);
            gjProject.InstrumentalTracks = new List<GjInstrumentalTracksItem>();
            int trackIndex = 0;
            foreach (var track in project.TrackList)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        GjSingingTracksItem gjTracksItem = EncodeSingingTrack(project, gjProject, ref noteID, trackID, singingTrack, timeSynchronizer);
                        gjProject.SingingTracks.Add(gjTracksItem);
                        trackID++;
                        break;
                    case InstrumentalTrack instrumentalTrack:
                        GjInstrumentalTracksItem gjAccompanimentsItem = new GjInstrumentalTracksItem();
                        EncodeInstrumentalTrack(project, trackID, instrumentalTrack, gjAccompanimentsItem);
                        gjProject.InstrumentalTracks.Add(gjAccompanimentsItem);
                        trackID++;
                        break;
                    default:
                        break;
                }
                trackIndex++;
            }
        }

        private static void EncodeInstrumentalTrack(Project project, int trackID, InstrumentalTrack instrumentalTrack, GjInstrumentalTracksItem gjAccompanimentsItem)
        {
            gjAccompanimentsItem.TrackID = Convert.ToString(trackID);
            gjAccompanimentsItem.Path = instrumentalTrack.AudioFilePath;
            //int offsetFromXS = instrumentalTrack.Offset;
            int convertedAccompanimentsOffset = 0;
            /*int tempoIndex = 0;
            int lastTempoPosition = 0;
            if (project.SongTempoList.Count == 1)
            {
                convertedAccompanimentsOffset += (int)((project.SongTempoList[0].BPM / 60.0) * offsetFromXS / 480.0 * 10000000.0);
            }
            else
            {
                while (project.SongTempoList[tempoIndex].Position <= offsetFromXS)
                {
                    convertedAccompanimentsOffset += (int)((project.SongTempoList[tempoIndex].BPM / 60.0) * (project.SongTempoList[tempoIndex].Position - lastTempoPosition) / 480.0 * 10000000.0);
                    lastTempoPosition = project.SongTempoList[tempoIndex].Position;
                    if (tempoIndex < project.SongTempoList.Count - 1)
                    {
                        tempoIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
                convertedAccompanimentsOffset += (int)((project.SongTempoList[tempoIndex].BPM / 60.0) * (offsetFromXS - lastTempoPosition) / 480.0 * 10000000.0);
            }*/
            gjAccompanimentsItem.Offset = convertedAccompanimentsOffset;
            gjAccompanimentsItem.TrackVolume = new GjTrackVolume
            {
                Volume = 1.0f,
                LeftVolume = 1.0f,
                RightVolume = 1.0f,
                Mute = instrumentalTrack.Mute
            };
            gjAccompanimentsItem.EQProgram = "";
        }

        private GjSingingTracksItem EncodeSingingTrack(Project project, GjProject gjProject, ref int noteID, int trackID, SingingTrack singingTrack, TimeSynchronizer timeSynchronizer)
        {
            GjSingingTracksItem gjTracksItem = new GjSingingTracksItem
            {
                TrackID = Convert.ToString(trackID),
                Name = "513singer",//扇宝
                NoteList = new List<GjNoteListItem>()
            };
            foreach (var note in singingTrack.NoteList)
            {
                EncodeNotes(project, gjProject, noteID, gjTracksItem, note, timeSynchronizer);
                noteID++;
            }
            EncodePitchParam(singingTrack, gjTracksItem);
            EncodeVolumeParam(singingTrack, gjTracksItem);
            EncodeSingsingTrackSettings(singingTrack, gjTracksItem);
            return gjTracksItem;
        }

        private void EncodeNotes(Project project, GjProject gjProject, int noteID, GjSingingTracksItem gjTracksItem, Note note, TimeSynchronizer timeSynchronizer)
        {
            GjNoteListItem gjBeatItemsItem = new GjNoteListItem
            {
                NoteID = noteID,
                Lyric = note.Lyric,
                Pinyin = note.Pronunciation ?? "",
                StartTick = note.StartPos + 1920 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator,
                Duration = note.Length,
                KeyNumber = note.KeyNumber - 24,
                PhonePreTime = 0,
                PhonePostTime = 0,
                Style = 0,
                //Style = GetNoteStyleFromXS(note.HeadTag)//当前歌叽歌叽版本不支持换气或停顿，暂不转换
            };
            gjTracksItem.NoteList.Add(gjBeatItemsItem);
        }

        private static void EncodeSingsingTrackSettings(SingingTrack singingTrack, GjSingingTracksItem gjTracksItem)
        {
            GjTrackVolume gjMasterVolume = new GjTrackVolume();
            gjTracksItem.TrackVolume = gjMasterVolume;
            gjTracksItem.TrackVolume.Volume = 1.0f;
            gjTracksItem.TrackVolume.LeftVolume = 1.0f;
            gjTracksItem.TrackVolume.RightVolume = 1.0f;
            gjTracksItem.TrackVolume.Mute = singingTrack.Mute;
            gjTracksItem.EQProgram = "";
        }

        private static void EncodeVolumeParam(SingingTrack singingTrack, GjSingingTracksItem gjTracksItem)
        {
            try
            {
                gjTracksItem.VolumeParam = new List<GjVolumeParamItem>();
                List<int> volumePointTimeBuffer = new List<int>();
                List<double> volumePointValueBuffer = new List<double>();
                int volumePointTimeFromXS;
                double volumeFromXS;
                double convertedVolumeValueFromXS;
                int lastVolumePointTimeFromXS = 0;

                for (int volumeParamPointIndex = 1; volumeParamPointIndex < singingTrack.EditedParams.Volume.PointList.Count - 1; volumeParamPointIndex++)
                {
                    volumePointTimeFromXS = singingTrack.EditedParams.Volume.PointList[volumeParamPointIndex].Item1;
                    volumeFromXS = singingTrack.EditedParams.Volume.PointList[volumeParamPointIndex].Item2;
                    convertedVolumeValueFromXS = (singingTrack.EditedParams.Volume.PointList[volumeParamPointIndex].Item2 + 1000.0) / 1000.0;

                    if (lastVolumePointTimeFromXS == singingTrack.EditedParams.Volume.PointList[volumeParamPointIndex].Item2)
                    {

                    }
                    else
                    {
                        if (volumeFromXS != 0)
                        {
                            volumePointTimeBuffer.Add(volumePointTimeFromXS);
                            volumePointValueBuffer.Add(convertedVolumeValueFromXS);
                        }
                        else
                        {
                            if (volumePointTimeBuffer.Count == 0 || volumePointValueBuffer.Count == 0)
                            {

                            }
                            else
                            {
                                for (int volumePointTimeBufferIndex = 0; volumePointTimeBufferIndex < volumePointTimeBuffer.Count; volumePointTimeBufferIndex += 5)
                                {
                                    GjVolumeParamItem gjVolumeMapItem = new GjVolumeParamItem
                                    {
                                        Time = volumePointTimeBuffer[volumePointTimeBufferIndex],
                                        Value = volumePointValueBuffer[volumePointTimeBufferIndex]
                                    };
                                    gjTracksItem.VolumeParam.Add(gjVolumeMapItem);
                                }

                                GjVolumeParamItem gjVolumeParamLeftEndpoint = new GjVolumeParamItem
                                {
                                    Time = volumePointTimeBuffer[0] - 5,
                                    Value = 1.0
                                };
                                gjTracksItem.VolumeParam.Add(gjVolumeParamLeftEndpoint);

                                GjVolumeParamItem gjVolumeParamRightEndpoint = new GjVolumeParamItem
                                {
                                    Time = volumePointTimeBuffer[volumePointTimeBuffer.Count - 1] + 5,
                                    Value = 1.0
                                };
                                gjTracksItem.VolumeParam.Add(gjVolumeParamRightEndpoint);


                                volumePointTimeBuffer.Clear();
                                volumePointValueBuffer.Clear();
                            }
                        }
                    }

                    lastVolumePointTimeFromXS = singingTrack.EditedParams.Volume.PointList[volumeParamPointIndex].Item1;
                }
            }
            catch (Exception)
            {

            }
        }

        private void EncodePitchParam(SingingTrack singingTrack, GjSingingTracksItem gjTracksItem)
        {
            try
            {
                gjTracksItem.PitchParam = new GjPitchParam();
                double convertedPitchFromXS;
                List<double> pitchPointBufferX = new List<double>();
                List<double> pitchPointBufferY = new List<double>();
                gjTracksItem.PitchParam.PitchPointList = new List<GjPitchPointListItem>();
                int lastPitchPointTimeFromXS = -100;
                int currentPitchFromXS;
                double convertedTimeFromXS;
                gjTracksItem.PitchParam.ModifyRanges = new List<GjModifyRangesItem>();
                for (int l = 0; l < singingTrack.EditedParams.Pitch.PointList.Count; l++)
                {
                    convertedTimeFromXS = (singingTrack.EditedParams.Pitch.PointList[l].Item1 / 5.0);
                    currentPitchFromXS = singingTrack.EditedParams.Pitch.PointList[l].Item2;
                    convertedPitchFromXS = ToneToY((double)((singingTrack.EditedParams.Pitch.PointList[l].Item2 - 5600.0) / 100.0));

                    if (lastPitchPointTimeFromXS == singingTrack.EditedParams.Pitch.PointList[l].Item1)
                    {

                    }
                    else
                    {
                        if (currentPitchFromXS != -100)
                        {
                            pitchPointBufferX.Add(convertedTimeFromXS);
                            pitchPointBufferY.Add(convertedPitchFromXS);
                        }
                        else
                        {
                            if (pitchPointBufferX.Count == 0 || pitchPointBufferY.Count == 0)
                            {

                            }
                            else
                            {
                                for (int j = 0; j < pitchPointBufferX.Count; j++)
                                {
                                    GjPitchPointListItem gjModifysItem = new GjPitchPointListItem
                                    {
                                        Time = pitchPointBufferX[j],
                                        Value = pitchPointBufferY[j]
                                    };
                                    gjTracksItem.PitchParam.PitchPointList.Add(gjModifysItem);
                                }
                                GjModifyRangesItem gjModifyRangesItem = new GjModifyRangesItem
                                {
                                    Left = pitchPointBufferX[0],
                                    Right = pitchPointBufferX[pitchPointBufferY.Count - 1]
                                };
                                gjTracksItem.PitchParam.ModifyRanges.Add(gjModifyRangesItem);
                                pitchPointBufferX.Clear();
                                pitchPointBufferY.Clear();
                            }
                        }
                    }

                    lastPitchPointTimeFromXS = singingTrack.EditedParams.Pitch.PointList[l].Item1;
                }
            }
            catch (Exception)
            {

            }
        }

        private static void EncodeTimeSignature(Project project, GjProject gjProject)
        {
            gjProject.TempoMap.TimeSignature = new List<GjTimeSignatureItem>(project.TimeSignatureList.Count);
            int sumOfTime = 0;
            for (int j = 0; j < project.TimeSignatureList.Count; j++)
            {
                GjTimeSignatureItem gjTimeSignatureItem = new GjTimeSignatureItem();
                if (j == 0)
                {
                    gjTimeSignatureItem.Time = project.TimeSignatureList[0].BarIndex * 1920 * project.TimeSignatureList[j].Numerator / project.TimeSignatureList[j].Denominator;
                }
                else
                {
                    sumOfTime += (project.TimeSignatureList[j].BarIndex - project.TimeSignatureList[j - 1].BarIndex) * 1920 * project.TimeSignatureList[j - 1].Numerator / project.TimeSignatureList[j - 1].Denominator;
                    gjTimeSignatureItem.Time = sumOfTime;
                }
                gjTimeSignatureItem.Numerator = project.TimeSignatureList[j].Numerator;
                gjTimeSignatureItem.Denominator = project.TimeSignatureList[j].Denominator;
                gjProject.TempoMap.TimeSignature.Add(gjTimeSignatureItem);
            }
        }

        private static void EncodeTempo(Project project, GjProject gjProject)
        {
            gjProject.TempoMap = new GjTempoMap
            {
                TicksPerQuarterNote = 480,
                Tempos = new List<GjTemposItem>(project.SongTempoList.Count)
            };
            for (int i = 0; i < project.SongTempoList.Count; i++)
            {
                GjTemposItem gjTemposItem = new GjTemposItem
                {
                    Time = project.SongTempoList[i].Position,
                    MicrosecondsPerQuarterNote = (int)(60.0 / project.SongTempoList[i].BPM * 1000000.0)
                };
                gjProject.TempoMap.Tempos.Add(gjTemposItem);
            }
        }

        private static void SetGjProjectProperties(GjProject gjProject)
        {
            gjProject.gjgjVersion = 1;
            gjProject.ProjectSetting = new GjProjectSetting
            {
                No1KeyName = "C",
                EQAfterMix = ""
            };
        }

        private double ToneToY(double tone)
        {
            return (71.0 - tone + 0.5) * 18.0;
        }

        private int GetNoteStyleFromXS(string origin)
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

    }
}