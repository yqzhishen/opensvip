using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenSvip.Library;
using OpenSvip.Model;
using SynthV.Model;
using SynthV.Options;
using SynthV.Param;
using SynthV.Utils;

namespace SynthV.Core
{
    public class SynthVDecoder
    {
        public PitchOptions PitchOption { get; set; }
        
        public bool ImportInstantPitch { get; set; }
        
        public BreathOptions BreathOption { get; set; }
        
        public GroupOptions GroupOption { get; set; }
        
        private int FirstBarTick;

        private double FirstBPM;

        private TimeSynchronizer Synchronizer;

        private SVVoice VoiceSettings;

        private SVParamCurve InstantPitch;

        private List<SVNote> NoteList;

        private List<string> LyricsPinyin;

        private readonly Dictionary<string, SVGroup> GroupLibrary = new Dictionary<string, SVGroup>();

        private readonly Dictionary<string, int> GroupSplitCounts = new Dictionary<string, int>();

        private readonly List<Track> TracksFromGroups = new List<Track>(); // reserved for note groups

        public Project DecodeProject(SVProject svProject)
        {
            var project = new Project();
            var time = svProject.Time;
            FirstBarTick = 1920 * time.Meters[0].Numerator / time.Meters[0].Denominator;
            FirstBPM = time.Tempos[0].BPM;
            
            project.SongTempoList = ScoreMarkUtils.ShiftTempoList(time.Tempos.ConvertAll(DecodeTempo), FirstBarTick);
            project.TimeSignatureList = ScoreMarkUtils.ShiftBeatList(time.Meters.ConvertAll(DecodeMeter), 1);
            Synchronizer = new TimeSynchronizer(project.SongTempoList);

            foreach (var svGroup in svProject.Library)
            {
                GroupLibrary[svGroup.UUID] = svGroup;
                GroupSplitCounts[svGroup.UUID] = 0;
            }
            
            foreach (var svTrack in svProject.Tracks)
            {
                project.TrackList.Add(DecodeTrack(svTrack));
            }
            project.TrackList.AddRange(TracksFromGroups);
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
                var masterNoteAttributes = VoiceSettings.ToAttributes();
                if (ImportInstantPitch)
                {
                    InstantPitch = track.MainRef.InstantPitch;
                }
                NoteList = track.MainGroup.Notes;
                NoteList.ForEach(note => note.MasterAttributes = masterNoteAttributes);
                var singingTrack = new SingingTrack
                {
                    NoteList = DecodeNoteList(track.MainGroup.Notes),
                    EditedParams = DecodeParams(track.MainGroup.Params)
                };
                switch (GroupOption)
                {
                    case GroupOptions.Split:
                        foreach (var svRef in track.Groups)
                        {
                            var group = GroupLibrary[svRef.GroupId] + svRef.BlickOffset ^ svRef.PitchOffset;
                            VoiceSettings = svRef.Voice;
                            masterNoteAttributes = VoiceSettings.ToAttributes();
                            if (ImportInstantPitch)
                            {
                                InstantPitch = svRef.InstantPitch;
                            }
                            NoteList = group.Notes;
                            NoteList.ForEach(note => note.MasterAttributes = masterNoteAttributes);
                            TracksFromGroups.Add(new SingingTrack
                            {
                                Title = $"{group.Name} ({++GroupSplitCounts[svRef.GroupId]})",
                                NoteList = DecodeNoteList(group.Notes),
                                EditedParams = DecodeParams(group.Params, track.MainGroup.Params)
                            });
                        }
                        break;
                    case GroupOptions.Merge:
                        var mergedGroup = track.MainGroup;
                        foreach (var svRef in track.Groups)
                        {
                            var group = GroupLibrary[svRef.GroupId] + svRef.BlickOffset ^ svRef.PitchOffset;
                            VoiceSettings = svRef.Voice;
                            masterNoteAttributes = VoiceSettings.ToAttributes();
                            if (ImportInstantPitch)
                            {
                                InstantPitch = svRef.InstantPitch;
                            }
                            NoteList = group.Notes;
                            NoteList.ForEach(note => note.MasterAttributes = masterNoteAttributes);
                            if (mergedGroup.IsOverlappedWith(group))
                            {
                                TracksFromGroups.Add(new SingingTrack
                                {
                                    Title = $"{group.Name} ({++GroupSplitCounts[svRef.GroupId]})",
                                    NoteList = DecodeNoteList(group.Notes),
                                    EditedParams = DecodeParams(group.Params, track.MainGroup.Params)
                                });
                            }
                            else
                            {
                                singingTrack.OverrideWith(
                                    DecodeNoteList(group.Notes),
                                    DecodeParams(group.Params, track.MainGroup.Params),
                                    FirstBarTick);
                                mergedGroup.Notes = mergedGroup.Notes
                                    .Concat(group.Notes)
                                    .OrderBy(note => note.Onset)
                                    .ToList();
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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

        private Params DecodeParams(SVParams svParams, SVParams masterParams = null)
        {
            var parameters = new Params
            {
                Pitch = DecodePitchCurve(
                    svParams.Pitch,
                    svParams.VibratoEnvelope,
                    5,
                    masterParams?.Pitch,
                    masterParams?.VibratoEnvelope),
                Volume = DecodeParamCurve(
                    svParams.Loudness,
                    val =>
                        val >= 0.0
                            ? (int) Math.Round(val / 12.0 * 1000.0)
                            : (int) Math.Round(1000.0 * Math.Pow(10, val / 20.0) - 1000.0),
                    VoiceSettings.MasterLoudness,
                    masterParams?.Loudness),
                Breath = DecodeParamCurve(
                    svParams.Breath,
                    val =>
                        (int) Math.Round(val * 1000.0),
                    VoiceSettings.MasterBreath,
                    masterParams?.Breath),
                Gender = DecodeParamCurve(
                    svParams.Gender,
                    val =>
                        (int) Math.Round(val * -1000.0),
                    VoiceSettings.MasterGender,
                    masterParams?.Gender),
                Strength = DecodeParamCurve(
                    svParams.Tension,
                    val =>
                        (int) Math.Round(val * 1000.0),
                    VoiceSettings.MasterTension,
                    masterParams?.Tension)
            };
            return parameters;
        }

        private ParamCurve DecodePitchCurve(
            SVParamCurve pitchDiff,
            SVParamCurve vibratoEnv,
            int step = 5,
            SVParamCurve masterPitchDiff = null,
            SVParamCurve masterVibratoEnv = null)
        {
            var curve = new ParamCurve();
            if (!NoteList.Any())
            {
                curve.PointList.Add(new Tuple<int, int>(-192000, -100));
                curve.PointList.Add(new Tuple<int, int>(1073741823, -100));
                return curve;
            }

            ParamExpression pitchDiffExpr = new CurveGenerator(pitchDiff.Points.ConvertAll(
                    point => new Tuple<int, int>(
                        DecodePosition(point.Item1),
                        (int) Math.Round(point.Item2))),
                DecodeInterpolation(pitchDiff.Mode));
            ParamExpression vibratoEnvExpr = new CurveGenerator(vibratoEnv.Points.ConvertAll(
                    point => new Tuple<int, int>(
                        DecodePosition(point.Item1),
                        (int) Math.Round(point.Item2 * 1000))),
                DecodeInterpolation(vibratoEnv.Mode),
                1000);
            if (masterPitchDiff != null)
            {
                pitchDiffExpr += new CurveGenerator(masterPitchDiff.Points.ConvertAll(
                        point => new Tuple<int, int>(
                            DecodePosition(point.Item1),
                            (int) Math.Round(point.Item2))),
                    DecodeInterpolation(masterPitchDiff.Mode));
            }
            if (masterVibratoEnv != null)
            {
                vibratoEnvExpr += new CurveGenerator(masterVibratoEnv.Points.ConvertAll(
                        point => new Tuple<int, int>(
                            DecodePosition(point.Item1),
                            (int) Math.Round(point.Item2 * 1000))),
                    DecodeInterpolation(vibratoEnv.Mode));
            }
            if (ImportInstantPitch)
            {
                pitchDiffExpr += new CurveGenerator(InstantPitch.Points.ConvertAll(
                        point => new Tuple<int, int>(
                            DecodePosition(point.Item1),
                            (int) Math.Round(point.Item2))),
                    DecodeInterpolation(InstantPitch.Mode));
            }
            var range = Range
                .Create(
                    NoteList.ConvertAll(note => new Tuple<int, int>(
                            DecodePosition(note.Onset),
                            DecodePosition(note.Onset + note.Duration)))
                        .ToArray())
                .Expand(120);
            switch (PitchOption)
            {
                case PitchOptions.Full:
                    break;
                case PitchOptions.Vibrato:
                case PitchOptions.Plain:
                    var regardDefaultVibratoAsUnedited = PitchOption == PitchOptions.Plain;
                    const double interval = 0.1;
                    var noteEditedRange = NoteList
                        .Where(note => note.PitchEdited(regardDefaultVibratoAsUnedited, ImportInstantPitch))
                        .Aggregate(
                            Range.Create(),
                            (current, note) =>
                            {
                                var startSecs =
                                    Synchronizer.GetActualSecsFromTicks(DecodePosition(note.Onset))
                                    - Math.Max(0.0, note.Attributes.TransitionOffset) - interval;
                                var endSecs =
                                    Synchronizer.GetActualSecsFromTicks(DecodePosition(note.Onset + note.Duration)) + interval;
                                return current.Union(Range.Create(new Tuple<int, int>(
                                    (int) Math.Round(Synchronizer.GetActualTicksFromSecs(Math.Max(0, startSecs))),
                                    (int) Math.Round(Synchronizer.GetActualTicksFromSecs(endSecs)))));
                            });
                    var paramEditedRange = pitchDiff.EditedRange() | vibratoEnv.EditedRange(1.0);
                    if (masterPitchDiff != null)
                    {
                        paramEditedRange |= masterPitchDiff.EditedRange();
                    }
                    if (masterVibratoEnv != null)
                    {
                        paramEditedRange |= masterVibratoEnv.EditedRange(1.0);
                    }
                    range &= noteEditedRange | paramEditedRange;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var generator = new PitchGenerator(Synchronizer, NoteList, pitchDiffExpr, vibratoEnvExpr);
            
            curve.PointList.Add(new Tuple<int, int>(-192000, -100));
            foreach (var (start, end) in range.Shift(FirstBarTick).SubRanges())
            {
                curve.PointList.Add(new Tuple<int, int>(start, -100));
                for (var i = start; i < end; i += step)
                {
                    curve.PointList.Add(new Tuple<int, int>(i, generator.ValueAtTicks(i - FirstBarTick)));
                }
                curve.PointList.Add(new Tuple<int, int>(end, generator.ValueAtTicks(end - FirstBarTick)));
                curve.PointList.Add(new Tuple<int, int>(end, -100));
            }
            curve.PointList.Add(new Tuple<int, int>(1073741823, -100));
            return curve;
        }

        private ParamCurve DecodeParamCurve(
            SVParamCurve svCurve,
            Func<double, int> mappingFunc,
            double baseValue = 0.0,
            SVParamCurve masterCurve = null)
        {
            int Clip(int val)
            {
                return Math.Max(-1000, Math.Min(1000, val));
            }
            
            var curve = new ParamCurve();
            var interpolation = DecodeInterpolation(svCurve.Mode);
            var decodedBaseValue = mappingFunc(baseValue);

            var generator = new CurveGenerator(
                svCurve.Points.ConvertAll(
                    point => new Tuple<int, int>(
                        DecodePosition(point.Item1) + FirstBarTick, mappingFunc(point.Item2 + baseValue))),
                interpolation,
                decodedBaseValue);
            if (masterCurve == null || !masterCurve.Points.Any())
            {
                // completely no edited parameter
                curve.PointList = generator
                    .GetConvertedCurve(5)
                    .ConvertAll(point => new Tuple<int, int>(point.Item1, Clip(point.Item2)));
                return curve;
            }
            
            if (!svCurve.Points.Any())
            {
                // no edited parameter in group; use master parameter
                curve.PointList = new CurveGenerator(
                    masterCurve.Points.ConvertAll(
                        point => new Tuple<int, int>(
                            DecodePosition(point.Item1) + FirstBarTick, mappingFunc(point.Item2 + baseValue))),
                    interpolation,
                    decodedBaseValue)
                    .GetConvertedCurve(5)
                    .ConvertAll(point => new Tuple<int, int>(point.Item1, Clip(point.Item2)));
                return curve;
            }
            // combine group & master parameters
            var groupExpr = new CurveGenerator(
                svCurve.Points.ConvertAll(
                    point => new Tuple<int, int>(
                        DecodePosition(point.Item1) + FirstBarTick,
                        (int) Math.Round(point.Item2 * 1000.0))),
                DecodeInterpolation(svCurve.Mode),
                (int) Math.Round(baseValue * 1000.0));
            var masterExpr =
                new CurveGenerator(masterCurve.Points.ConvertAll(
                        point => new Tuple<int, int>(
                            DecodePosition(point.Item1) + FirstBarTick,
                            (int) Math.Round(point.Item2 * 1000.0))),
                    DecodeInterpolation(masterCurve.Mode));
            var compoundExpr = groupExpr + masterExpr;
            var groupPoints = groupExpr.PointList;
            var masterPoints = masterExpr.PointList;
            
            int ActualValueAt(int ticks)
            {
                return Clip(mappingFunc(compoundExpr.ValueAtTicks(ticks) / 1000.0));
            }

            int i = 0, j = 0;
            Tuple<int, int> prevPoint;
            bool prevPointIsBase;
            if (groupPoints[i].Item1 <= masterPoints[j].Item1)
            {
                prevPoint = groupPoints[i++];
                prevPointIsBase = prevPoint.Item2 == (int) Math.Round(baseValue * 1000.0);
            }
            else
            {
                prevPoint = masterPoints[j++];
                prevPointIsBase = prevPoint.Item2 == 0;
            }
            curve.PointList.Add(new Tuple<int, int>(-192000, ActualValueAt(0)));
            curve.PointList.Add(new Tuple<int, int>(prevPoint.Item1, ActualValueAt(prevPoint.Item1)));
            while (i < groupPoints.Count || j < masterPoints.Count)
            {
                Tuple<int, int> currentPoint;
                bool currentPointIsBase;
                if (i < groupPoints.Count
                    && (j >= masterPoints.Count || groupPoints[i].Item1 <= masterPoints[j].Item1))
                {
                    currentPoint = groupPoints[i++];
                    currentPointIsBase = currentPoint.Item2 == (int) Math.Round(baseValue * 1000.0);
                }
                else
                {
                    currentPoint = masterPoints[j++];
                    currentPointIsBase = currentPoint.Item2 == 0;
                }

                if (prevPointIsBase && currentPointIsBase && prevPoint.Item1 < currentPoint.Item1)
                {
                    curve.PointList.Add(new Tuple<int, int>(prevPoint.Item1, ActualValueAt(prevPoint.Item1)));
                    curve.PointList.Add(new Tuple<int, int>(currentPoint.Item1, ActualValueAt(currentPoint.Item1)));
                }
                else
                {
                    for (var p = prevPoint.Item1; p < currentPoint.Item1; p += 5)
                    {
                        curve.PointList.Add(new Tuple<int, int>(p, ActualValueAt(p)));
                    }
                }
                
                prevPoint = currentPoint;
                prevPointIsBase = currentPointIsBase;
            }
            curve.PointList.Add(new Tuple<int, int>(prevPoint.Item1, ActualValueAt(prevPoint.Item1)));
            curve.PointList.Add(new Tuple<int, int>(1073741823, ActualValueAt(prevPoint.Item1)));
            return curve;
        }

        private Func<double, double> DecodeInterpolation(string mode)
        {
            switch (mode)
            {
                case "cosine":
                    return Interpolation.CosineInterpolation();
                case "cubic":
                    return Interpolation.CubicInterpolation();
                default:
                    return Interpolation.LinearInterpolation();
            }
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
                    && currentDuration.Length > index;
                var nextHeadPartEdited =
                    nextPhoneMarks[0] > 0
                    && nextDuration != null
                    && nextDuration.Any();

                if (currentMainPartEdited && currentDuration.Length > index + 1)
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
                    var spaceInSecs = Math.Min(2.0, Synchronizer.GetDurationSecsFromTicks(
                        noteList[i].StartPos + FirstBarTick,
                        noteList[i + 1].StartPos + FirstBarTick));
                    var length = nextPhoneMarks[0] * nextDuration[0];
                    if (currentMainPartEdited)
                    {
                        var ratio = currentDuration.Length > index + 1
                            ? 2 / (currentDuration[index] + currentDuration[index + 1])
                            : 1 / currentDuration[index];
                        if (length * ratio > Synchronizer.GetDurationSecsFromTicks(
                                noteList[i].StartPos + noteList[i].Length + FirstBarTick,
                                noteList[i + 1].StartPos + FirstBarTick))
                        {
                            length *= ratio;
                        }
                    }
                    noteList[i + 1].EditedPhones.HeadLengthInSecs = (float) Math.Min(0.9 * spaceInSecs, length);
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
            note.Lyric = svNote.Lyrics;
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
