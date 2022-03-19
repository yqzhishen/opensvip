using System;
using OpenSvip.Model;
using Gjgj.Model;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Plugin.Gjgj
{
    public class GjgjDecoder
    {
        public Project DecodeProject(GjProject gjProject)
        {
            try
            {
                var project = new Project();
                project.Version = "SVIP7.0.0";
                DecodeTempo(gjProject, project);
                DecodeTimeSignature(gjProject, project);
                DecodeTracks(gjProject, project);
                return project;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        private void DecodeTracks(GjProject gjProject, Project project)
        {
            DecodeSingingTracks(gjProject, project);
            DecodeInstrumentalTracks(gjProject, project);
        }

        private static void DecodeInstrumentalTracks(GjProject gjProject, Project project)
        {
            for (int instrumentalTrackIndex = 0; instrumentalTrackIndex < gjProject.Accompaniments.Count; instrumentalTrackIndex++)
            {
                Track svipTrack = new InstrumentalTrack
                {
                    Title = Path.GetFileNameWithoutExtension(gjProject.Accompaniments[instrumentalTrackIndex].Path),
                    Mute = gjProject.Accompaniments[instrumentalTrackIndex].MasterVolume.Mute,
                    Solo = false,
                    Volume = 0.3,
                    Pan = 0.0,
                    AudioFilePath = gjProject.Accompaniments[instrumentalTrackIndex].Path,
                    Offset = 0
                };
                project.TrackList.Add(svipTrack);
            }
        }

        private void DecodeSingingTracks(GjProject gjProject, Project project)
        {
            for (int singingTrackIndex = 0; singingTrackIndex < gjProject.Tracks.Count; singingTrackIndex++)
            {
                List<Note> noteListFromGj = new List<Note>();
                for (int noteIndex = 0; noteIndex < gjProject.Tracks[singingTrackIndex].BeatItems.Count; noteIndex++)
                {
                    Note noteFromGj = DecodeNote(gjProject, singingTrackIndex, noteIndex);
                    //MessageBox.Show(noteFromGj.Lyric);
                    noteListFromGj.Add(noteFromGj);
                }

                Track svipTrack = new SingingTrack
                {
                    Title = GetSingerNameFromGj(gjProject.Tracks[singingTrackIndex].Name),
                    Mute = gjProject.Tracks[singingTrackIndex].MasterVolume.Mute,
                    Solo = false,
                    Volume = 0.7,
                    Pan = 0.0,
                    AISingerName = "陈水若",
                    ReverbPreset = "干声",
                    NoteList = noteListFromGj
                };
                project.TrackList.Add(svipTrack);
            }
        }

        private static Note DecodeNote(GjProject gjProject, int singingTrackIndex, int noteIndex)
        {
            return new Note
            {
                StartPos = gjProject.Tracks[singingTrackIndex].BeatItems[noteIndex].StartTick - 1920 * gjProject.TempoMap.TimeSignature[0].Numerator / gjProject.TempoMap.TimeSignature[0].Denominator,
                Length = gjProject.Tracks[singingTrackIndex].BeatItems[noteIndex].Duration,
                KeyNumber = gjProject.Tracks[singingTrackIndex].BeatItems[noteIndex].Track + 24,
                Lyric = gjProject.Tracks[singingTrackIndex].BeatItems[noteIndex].Lyric,
                Pronunciation = gjProject.Tracks[singingTrackIndex].BeatItems[noteIndex].Pinyin ?? null,
            };
        }

        private static void DecodeTimeSignature(GjProject gjProject, Project project)
        {
            if(gjProject.TempoMap.TimeSignature.Count == 0)
            {
                TimeSignature timeSignature = new TimeSignature
                {
                    BarIndex = 0,
                    Numerator = 4,
                    Denominator = 4
                };
                project.TimeSignatureList.Add(timeSignature);
            }
            else
            {
                for (int timeSignatureIndex = 0; timeSignatureIndex < gjProject.TempoMap.TimeSignature.Count; timeSignatureIndex++)
                {
                    TimeSignature timeSignature = new TimeSignature();
                    if (timeSignatureIndex == 0)
                    {
                        timeSignature.BarIndex = gjProject.TempoMap.TimeSignature[0].Time / 1920 / gjProject.TempoMap.TimeSignature[timeSignatureIndex].Numerator * gjProject.TempoMap.TimeSignature[timeSignatureIndex].Denominator;
                    }
                    else
                    {
                        //TODO: 多拍号导入
                        timeSignature.BarIndex = gjProject.TempoMap.TimeSignature[0].Time / 1920 / gjProject.TempoMap.TimeSignature[timeSignatureIndex].Numerator * gjProject.TempoMap.TimeSignature[timeSignatureIndex].Denominator;
                    }
                    timeSignature.Numerator = gjProject.TempoMap.TimeSignature[timeSignatureIndex].Numerator;
                    timeSignature.Denominator = gjProject.TempoMap.TimeSignature[timeSignatureIndex].Denominator;
                    project.TimeSignatureList.Add(timeSignature);
                }
            }
            
        }

        private static void DecodeTempo(GjProject gjProject, Project project)
        {
            for (int i = 0; i < gjProject.TempoMap.Tempos.Count; i++)
            {
                SongTempo songTempo = new SongTempo
                {
                    Position = gjProject.TempoMap.Tempos[i].Time,
                    BPM = (float)(60.0 / gjProject.TempoMap.Tempos[i].MicrosecondsPerQuarterNote * 1000000.0)
                };
                project.SongTempoList.Add(songTempo);
            }
        }

        private string GetSingerNameFromGj(string origin)
        {
            switch (origin)
            {
                case "513singer":
                    return "扇宝";
                case "514singer":
                    return "SING-林嘉慧";
                default:
                    return "";
            }
        }
    }
}