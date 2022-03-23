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
        
        public PitchOptions PitchOption { get; set; }
        
        private int FirstBarTick;

        private double FirstBPM;

        private TimeSynchronizer Synchronizer;

        private SVVoice VoiceSettings;

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
            Synchronizer = new TimeSynchronizer(project.SongTempoList);
            
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
                VoiceSettings = track.MainRef.Voice;
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
                Volume = DecodeParamCurve(svParams.Loudness, VoiceSettings.MasterLoudness, val =>
                    val >= 0.0
                        ? (int) Math.Round(val / 12.0 * 1000.0)
                        : (int) Math.Round(1000.0 * Math.Pow(10, val / 20.0) - 1000.0)),
                Breath = DecodeParamCurve(svParams.Breath, VoiceSettings.MasterBreath, val =>
                    (int) Math.Round(val * 1000.0)),
                Gender = DecodeParamCurve(svParams.Gender, VoiceSettings.MasterGender, val =>
                    (int) Math.Round(val * -1000.0)),
                Strength = DecodeParamCurve(svParams.Tension, VoiceSettings.MasterTension, val =>
                    (int) Math.Round(val * 1000.0))
            };
            return parameters;
        }

        private ParamCurve DecodeParamCurve(SVParamCurve svCurve, double baseValue,
            Func<double, int> mappingFunc)
        {
            int Clip(int val)
            {
                return Math.Max(-1000, Math.Min(1000, val));
            }
            
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
                    point => new Tuple<int, int>(
                        DecodePosition(point.Item1) + FirstBarTick, Clip(mappingFunc(point.Item2 + baseValue)))),
                interpolation);
            curve.PointList = generator.GetCurve(5, Clip(mappingFunc(baseValue)));
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
            LyricsPinyin = PhonemeUtils.LyricsToPinyin(noteList.ConvertAll(note =>
            {
                if (!string.IsNullOrEmpty(note.Pronunciation))
                {
                    return note.Pronunciation;
                }
                var validChars = Regex.Replace(
                    note.Lyric,
                    "[\\s\\(\\)\\[\\]\\{\\}\\^_*×――—（）$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”0-9a-zA-Z]",
                    "la");
                return validChars == "" ? "la" : validChars[0].ToString();
            }));
            if (!noteList.Any())
            {
                return noteList;
            }

            var currentSVNote = svNotes[0];
            var currentDuration = currentSVNote.Attributes.PhoneDurations;
            var currentPhoneMarks = PhonemeUtils.DefaultPhoneMarks(LyricsPinyin[0]);
            // head part of the first note
            if (currentPhoneMarks[0] > 0 && currentDuration != null && !currentDuration[0].Equals(1.0))
            {
                noteList[0].EditedPhones = new Phones
                {
                    HeadLengthInSecs = (float) Math.Min(1.8, currentPhoneMarks[0] * currentDuration[0])
                };
            }
            var i = 0;
            for (; i < svNotes.Count - 1; i++)
            {
                var nextSVNote = svNotes[i + 1];
                var nextDuration = nextSVNote.Attributes.PhoneDurations;
                var nextPhoneMarks = PhonemeUtils.DefaultPhoneMarks(LyricsPinyin[i + 1]);

                var index = currentPhoneMarks[0] > 0 ? 1 : 0;
                var currentMainPartEdited =
                    currentPhoneMarks[1] > 0
                    && currentDuration != null
                    && currentDuration.Length > index + 1;
                var nextHeadPartEdited =
                    nextPhoneMarks[0] > 0
                    && nextDuration != null
                    && nextDuration.Any();

                if (currentMainPartEdited)
                {
                    if (noteList[i].EditedPhones == null)
                    {
                        noteList[i].EditedPhones = new Phones();
                    }
                    noteList[i].EditedPhones.MidRatioOverTail =
                        (float) (currentPhoneMarks[1] * currentDuration[index] / currentDuration[index + 1]);
                }

                if (nextHeadPartEdited)
                {
                    if (noteList[i + 1].EditedPhones == null)
                    {
                        noteList[i + 1].EditedPhones = new Phones();
                    }
                    var spaceInSecs = Synchronizer.GetDurationSecsFromTicks(
                        noteList[i].StartPos + FirstBarTick,
                        noteList[i + 1].StartPos + FirstBarTick);
                    var ratio = nextPhoneMarks[0] * nextDuration[0];
                    if (currentMainPartEdited)
                    {
                        ratio *= 2 / (currentDuration[index] + currentDuration[index + 1]);
                    }
                    noteList[i + 1].EditedPhones.HeadLengthInSecs = (float) Math.Min(0.8 * spaceInSecs, ratio);
                }
                
                currentDuration = nextDuration;
                currentPhoneMarks = nextPhoneMarks;
            }
            // main part of the last note
            var idx = currentPhoneMarks[0] > 0 ? 1 : 0;
            if (currentPhoneMarks[1] > 0 && currentDuration != null && currentDuration.Length > idx + 1)
            {
                if (noteList[i].EditedPhones == null)
                {
                    noteList[i].EditedPhones = new Phones();
                }
                noteList[i].EditedPhones.MidRatioOverTail =
                    (float) (currentPhoneMarks[1] * currentDuration[idx] / currentDuration[idx + 1]);
            }
            
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
            if (!string.IsNullOrEmpty(svNote.Phonemes))
            {
                note.Lyric = "啊";
                note.Pronunciation = PhonemeUtils.XsampaToPinyin(svNote.Phonemes);
            }
            else if (Regex.IsMatch(svNote.Lyrics, @"[a-zA-Z]"))
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
            return (int) Math.Round(offset / 1470000.0 * FirstBPM / 120.0);
        }

        private int DecodePosition(long position)
        {
            return (int) Math.Round(position / 1470000.0);
        }
    }
}
