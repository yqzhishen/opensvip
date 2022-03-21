using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenSvip.Library;
using OpenSvip.Model;
using SynthV.Model;

namespace Plugin.SynthV
{
    public class SynthVDecoder
    {
        public BreathOptions BreathOption { get; set; }
        
        private int FirstBarTick;

        private double FirstBPM;

        private List<string> LyricsPinyin;

        private List<Track> GroupTracksBuffer = new List<Track>(); // reserved for note groups

        public Project DecodeProject(SVProject svProject)
        {
            var project = new Project();
            var time = svProject.Time;
            FirstBarTick = 1920 * time.Meters[0].Numerator / time.Meters[0].Denominator;
            FirstBPM = time.Tempos[0].BPM;
            
            project.SongTempoList = ScoreMarkUtils.ShiftTempoList(time.Tempos.ConvertAll(DecodeTempo), FirstBarTick);
            project.TimeSignatureList = ScoreMarkUtils.ShiftBeatList(time.Meters.ConvertAll(DecodeMeter), 1);
            
            foreach (var svTrack in svProject.Tracks)
            {
                project.TrackList.Add(DecodeTrack(svTrack));
            }
            return project;
        }

        private TimeSignature DecodeMeter(SVMeter meter)
        {
            return new TimeSignature
            {
                BarIndex = meter.Index,
                Numerator = meter.Numerator,
                Denominator = meter.Denominator
            };
        }

        private SongTempo DecodeTempo(SVTempo tempo)
        {
            return new SongTempo
            {
                Position = DecodePosition(tempo.Position),
                BPM = (float) tempo.BPM
            };
        }

        private Track DecodeTrack(SVTrack track)
        {
            Track svipTrack;
            if (track.MainRef.IsInstrumental)
            {
                svipTrack = new InstrumentalTrack
                {
                    AudioFilePath = track.MainRef.Audio.Filename,
                    Offset = DecodeAudioOffset(track.MainRef.BlickOffset)
                };
            }
            else
            {
                var singingTrack = new SingingTrack
                {
                    NoteList = DecodeNoteList(track.MainGroup.Notes),
                    EditedParams = DecodeParams(track.MainGroup.Params)
                };
                // TODO: decode note groups
                svipTrack = singingTrack;
            }
            svipTrack.Title = track.Name;
            svipTrack.Mute = track.Mixer.Mute;
            svipTrack.Solo = track.Mixer.Solo;
            svipTrack.Volume = DecodeVolume(track.Mixer.GainDecibel);
            svipTrack.Pan = track.Mixer.Pan;
            return svipTrack;
        }

        private double DecodeVolume(double gain)
        {
            return gain >= 0
                ? Math.Min(gain / (20 * Math.Log10(4)) + 1.0, 2.0)
                : Math.Pow(10, gain / 20.0);
        }

        private Params DecodeParams(SVParams svParams)
        {
            var parameters = new Params
            {
                // TODO: decode pitch
                Volume = DecodeParamCurve(svParams.Loudness, 0,
                    val => val >= 0.0
                        ? (int) Math.Round(val / 12.0 * 1000.0)
                        : (int) Math.Round(1000.0 * Math.Pow(10, val / 20.0) - 1000.0)),
                Breath = DecodeParamCurve(svParams.Breath, 0,
                    val => (int) Math.Round(val * 1000.0)),
                Gender = DecodeParamCurve(svParams.Gender, 0,
                    val => (int) Math.Round(-val * 1000.0)),
                Strength = DecodeParamCurve(svParams.Tension, 0,
                    val => (int) Math.Round(val * 1000.0))
            };
            return parameters;
        }

        private ParamCurve DecodeParamCurve(SVParamCurve svCurve, int termination, Func<double, int> op)
        {
            var curve = new ParamCurve();
            Func<double, double> interpolation;
            switch (svCurve.Mode)
            {
                case "cosine":
                    interpolation = Interpolation.CosineInterpolation();
                    break;
                case "cubic":
                    interpolation = Interpolation.CubicInterpolation();
                    break;
                default:
                    interpolation = Interpolation.LinearInterpolation();
                    break;
            }

            var generator = new CurveGenerator(
                svCurve.Points.ConvertAll(
                    point => new Tuple<int, int>(DecodePosition(point.Item1) + FirstBarTick, op(point.Item2))),
                interpolation);
            curve.PointList = generator.GetCurve(5, termination);
            return curve;
        }

        private List<Note> DecodeNoteList(List<SVNote> svNotes)
        {
            List<Note> noteList;
            const string breathPattern = @"^\s*\.?\s*br(l?[1-9])?\s*$";
            switch (BreathOption)
            {
                case BreathOptions.Ignore:
                    svNotes = svNotes.Where(note => !Regex.IsMatch(note.Lyrics, breathPattern)).ToList();
                    goto case BreathOptions.Remain;
                case BreathOptions.Remain:
                    noteList = svNotes.Select(DecodeNote).ToList();
                    break;
                case BreathOptions.Convert:
                    noteList = new List<Note>();
                    if (svNotes.Count > 1)
                    {
                        var prevIndex = -1;
                        int currentIndex;
                        while (-1 != (currentIndex = svNotes.FindIndex(prevIndex + 1, 
                                   svNote => !Regex.IsMatch(svNote.Lyrics, breathPattern))))
                        {
                            var note = DecodeNote(svNotes[currentIndex]);
                            if (currentIndex > prevIndex + 1)
                            {
                                var breathNote = svNotes[currentIndex - 1];
                                if (DecodePosition(svNotes[currentIndex].Onset) - DecodePosition(breathNote.Onset + breathNote.Duration) <= 120)
                                {
                                    note.HeadTag = "V";
                                }
                            }
                            noteList.Add(note);
                            prevIndex = currentIndex;
                        }
                    }
                    else if (svNotes.Count == 1 && !Regex.IsMatch(svNotes[0].Lyrics, breathPattern))
                    {
                        noteList.Add(DecodeNote(svNotes[0]));
                    }
                    svNotes = svNotes.Where(note => !Regex.IsMatch(note.Lyrics, breathPattern)).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // TODO: decode phones
            return noteList;
        }

        private Note DecodeNote(SVNote svNote)
        {
            var note = new Note
            {
                StartPos = DecodePosition(svNote.Onset),
                KeyNumber = svNote.Pitch
            };
            note.Length = DecodePosition(svNote.Onset + svNote.Duration) - note.StartPos; // avoid overlapping
            if (Regex.IsMatch(svNote.Lyrics, @"[a-zA-Z]"))
            {
                note.Lyric = "啊";
                note.Pronunciation = svNote.Lyrics;
            }
            else
            {
                note.Lyric = svNote.Lyrics;
            }
            return note;
        }

        private int DecodeAudioOffset(long offset)
        {
            if (offset >= 0)
            {
                return DecodePosition(offset);
            }
            return (int) Math.Round(offset / 1470000.0 * (FirstBPM / 120));
        }

        private int DecodePosition(long position)
        {
            return (int) Math.Round(position / 1470000.0);
        }
    }
}
