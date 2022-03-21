using System;
using System.Collections.Generic;
using System.Linq;
using OpenSvip.Model;
using Gjgj.Model;
using System.Windows.Forms;

namespace Plugin.Gjgj
{
    public class GjgjEncoder
    {
        public GjProject EncodeProject(Project project)
        {
            try
            {
                GjProject gjProject = new GjProject();
                SetGjProjectProperties(gjProject);
                EncodeTempo(project, gjProject);
                EncodeTimeSignature(project, gjProject);
                EncodeTracks(project, gjProject);
                return gjProject;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void EncodeTracks(Project project, GjProject gjProject)
        {
            int noteID = 1;
            int trackID = 1;
            gjProject.Tracks = new List<GjTracksItem>(project.TrackList.Count);
            gjProject.Accompaniments = new List<GjAccompanimentsItem>();
            int trackIndex = 0;
            foreach (var track in project.TrackList)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        GjTracksItem gjTracksItem = EncodeSingingTrack(project, ref noteID, trackID, singingTrack);
                        gjProject.Tracks.Add(gjTracksItem);
                        trackID++;
                        break;
                    case InstrumentalTrack instrumentalTrack:
                        GjAccompanimentsItem gjAccompanimentsItem = new GjAccompanimentsItem();
                        EncodeInstrumentalTrack(project, trackID, instrumentalTrack, gjAccompanimentsItem);
                        gjProject.Accompaniments.Add(gjAccompanimentsItem);
                        trackID++;
                        break;
                    default:
                        break;
                }
                trackIndex++;
            }
        }

        private static void EncodeInstrumentalTrack(Project project, int trackID, InstrumentalTrack instrumentalTrack, GjAccompanimentsItem gjAccompanimentsItem)
        {
            gjAccompanimentsItem.ID = Convert.ToString(trackID);
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
            gjAccompanimentsItem.MasterVolume = new GjMasterVolume
            {
                Volume = 1.0f,
                LeftVolume = 1.0f,
                RightVolume = 1.0f,
                Mute = instrumentalTrack.Mute
            };
            gjAccompanimentsItem.EQProgram = "";
        }

        private GjTracksItem EncodeSingingTrack(Project project, ref int noteID, int trackID, SingingTrack singingTrack)
        {
            GjTracksItem gjTracksItem = new GjTracksItem
            {
                ID = Convert.ToString(trackID),
                Name = "513singer",//扇宝
                BeatItems = new List<GjBeatItemsItem>()
            };
            foreach (var note in singingTrack.NoteList)
            {
                EncodeNotes(project, noteID, gjTracksItem, note);
                noteID++;
            }
            EncodePitchParam(singingTrack, gjTracksItem);
            EncodeVolumeParam(singingTrack, gjTracksItem);
            EncodeSingsingTrackSettings(singingTrack, gjTracksItem);
            return gjTracksItem;
        }

        private static void EncodeNotes(Project project, int noteID, GjTracksItem gjTracksItem, Note note)
        {
            GjBeatItemsItem gjBeatItemsItem = new GjBeatItemsItem
            {
                ID = noteID,
                Lyric = note.Lyric,
                Pinyin = note.Pronunciation ?? "",
                StartTick = note.StartPos + 1920 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator,
                Duration = note.Length,
                Track = note.KeyNumber - 24,
                PreTime = 0,
                PostTime = 0,
                Style = 0
            };
            gjTracksItem.BeatItems.Add(gjBeatItemsItem);
        }

        private static void EncodeSingsingTrackSettings(SingingTrack singingTrack, GjTracksItem gjTracksItem)
        {
            GjMasterVolume gjMasterVolume = new GjMasterVolume();
            gjTracksItem.MasterVolume = gjMasterVolume;
            gjTracksItem.MasterVolume.Volume = 1.0f;
            gjTracksItem.MasterVolume.LeftVolume = 1.0f;
            gjTracksItem.MasterVolume.RightVolume = 1.0f;
            gjTracksItem.MasterVolume.Mute = singingTrack.Mute;
            gjTracksItem.EQProgram = "";
        }

        private static void EncodeVolumeParam(SingingTrack singingTrack, GjTracksItem gjTracksItem)
        {
            gjTracksItem.VolumeMap = new List<GjVolumeMapItem>();
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
                                GjVolumeMapItem gjVolumeMapItem = new GjVolumeMapItem
                                {
                                    Time = volumePointTimeBuffer[volumePointTimeBufferIndex],
                                    Volume = volumePointValueBuffer[volumePointTimeBufferIndex]
                                };
                                gjTracksItem.VolumeMap.Add(gjVolumeMapItem);
                            }

                            GjVolumeMapItem gjVolumeParamLeftEndpoint = new GjVolumeMapItem
                            {
                                Time = volumePointTimeBuffer[0] - 5,
                                Volume = 1.0
                            };
                            gjTracksItem.VolumeMap.Add(gjVolumeParamLeftEndpoint);

                            GjVolumeMapItem gjVolumeParamRightEndpoint = new GjVolumeMapItem
                            {
                                Time = volumePointTimeBuffer[volumePointTimeBuffer.Count - 1] + 5,
                                Volume = 1.0
                            };
                            gjTracksItem.VolumeMap.Add(gjVolumeParamRightEndpoint);


                            volumePointTimeBuffer.Clear();
                            volumePointValueBuffer.Clear();
                        }
                    }
                }

                lastVolumePointTimeFromXS = singingTrack.EditedParams.Volume.PointList[volumeParamPointIndex].Item1;
            }
        }

        private void EncodePitchParam(SingingTrack singingTrack, GjTracksItem gjTracksItem)
        {
            gjTracksItem.Tone = new GjTone();
            double convertedPitchFromXS;
            List<double> pitchPointBufferX = new List<double>();
            List<double> pitchPointBufferY = new List<double>();
            gjTracksItem.Tone.Modifys = new List<GjModifysItem>();
            int lastPitchPointTimeFromXS = -100;
            int currentPitchFromXS;
            double convertedTimeFromXS;
            gjTracksItem.Tone.ModifyRanges = new List<GjModifyRangesItem>();
            for (int l = 0; l < singingTrack.EditedParams.Pitch.PointList.Count; l++)
            {
                convertedTimeFromXS = (singingTrack.EditedParams.Pitch.PointList[l].Item1 / 5.0);
                currentPitchFromXS = singingTrack.EditedParams.Pitch.PointList[l].Item2;
                convertedPitchFromXS = ToneToY((double)((singingTrack.EditedParams.Pitch.PointList[l].Item2 - 2400.0) / 100.0));

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
                                GjModifysItem gjModifysItem = new GjModifysItem
                                {
                                    X = pitchPointBufferX[j],
                                    Y = pitchPointBufferY[j]
                                };
                                gjTracksItem.Tone.Modifys.Add(gjModifysItem);
                            }
                            GjModifyRangesItem gjModifyRangesItem = new GjModifyRangesItem
                            {
                                X = pitchPointBufferX[0],
                                Y = pitchPointBufferX[pitchPointBufferY.Count - 1]
                            };
                            gjTracksItem.Tone.ModifyRanges.Add(gjModifyRangesItem);
                            pitchPointBufferX.Clear();
                            pitchPointBufferY.Clear();
                        }
                    }
                }

                lastPitchPointTimeFromXS = singingTrack.EditedParams.Pitch.PointList[l].Item1;
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
    }
}