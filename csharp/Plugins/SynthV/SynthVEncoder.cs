using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NAudio.Wave;
using OpenSvip.Model;
using OpenSvip.Library;
using SynthV.Model;

namespace Plugin.SynthV
{
    public class SynthVEncoder
    {
        public int DefaultTempo { get; set; } = 60;

        public VibratoOptions VibratoOptions { get; set; } = VibratoOptions.Hybrid;

        private int FirstBarTick;

        private List<SongTempo> FirstBarTempo;

        private List<Note> NoteBuffer;

        private TimeSynchronizer Synchronizer;

        private PitchGenerator PitchGenerator;

        private HashSet<int> NoVibratoIndexes;

        private List<string> LyricsPinyin;

        public SVProject EncodeProject(Project project)
        {
            var svProject = new SVProject();
            var newMeters = project.TimeSignatureList.Skip(1).Select(
                meter => new TimeSignature
                {
                    BarIndex = meter.BarIndex - 1,
                    Numerator = meter.Numerator,
                    Denominator = meter.Denominator
                }).ToList();
            if (!newMeters.Any() || newMeters[0].BarIndex > 0)
            {
                newMeters.Insert(0, new TimeSignature
                {
                    BarIndex = 0,
                    Numerator = project.TimeSignatureList[0].Numerator,
                    Denominator = project.TimeSignatureList[0].Denominator
                });
            }
            FirstBarTick = (int) Math.Round(1920.0 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator);
            FirstBarTempo = project.SongTempoList.Where(tempo => tempo.Position < FirstBarTick).ToList();
            var isAbsoluteTimeMode = newMeters.Any(meter => meter.Denominator < 2 || meter.Denominator > 16);
            Synchronizer = new TimeSynchronizer(project.SongTempoList, FirstBarTick, isAbsoluteTimeMode, DefaultTempo);
            if (!isAbsoluteTimeMode)
            {
                foreach (var meter in newMeters)
                {
                    svProject.Time.Meters.Add(EncodeMeter(meter));
                }
                foreach (var tempo in Synchronizer.TempoListAfterOffset)
                {
                    svProject.Time.Tempos.Add(EncodeTempo(tempo));
                }
            }
            else
            {
                svProject.Time.Meters.Add(new SVMeter
                {
                    Index = 0,
                    Numerator = 4,
                    Denominator = 4
                });
                svProject.Time.Tempos.Add(new SVTempo
                {
                    Position = 0,
                    BPM = DefaultTempo
                });
            }
            
            var id = 0;
            foreach (var svTrack in project.TrackList.Select(EncodeTrack))
            {
                svTrack.DispOrder = id;
                svProject.Tracks.Add(svTrack);
                ++id;
            }
            return svProject;
        }

        private SVMeter EncodeMeter(TimeSignature signature)
        {
            return new SVMeter
            {
                Index = signature.BarIndex,
                Numerator = signature.Numerator,
                Denominator = signature.Denominator
            };
        }

        private SVTempo EncodeTempo(SongTempo tempo)
        {
            return new SVTempo
            {
                Position = EncodePosition(tempo.Position),
                BPM = tempo.BPM
            };
        }

        private SVTrack EncodeTrack(Track track)
        {
            var svTrack = new SVTrack
            {
                Name = track.Title,
                Mixer =
                {
                    GainDecibel = EncodeVolume(track.Volume),
                    Pan = track.Pan,
                    Mute = track.Mute,
                    Solo = track.Solo
                }
            };
            switch (track)
            {
                case SingingTrack singingTrack:
                    svTrack.MainRef.IsInstrumental = false;
                    svTrack.DispColor = "ff7db235";
                    svTrack.MainRef.Database.Language = "mandarin";
                    svTrack.MainRef.Database.PhoneSet = "xsampa";
                    
                    NoteBuffer = singingTrack.NoteList;
                    PitchGenerator = new PitchGenerator(
                        singingTrack.NoteList,
                        Synchronizer,
                        PitchInterpolation.SigmoidInterpolation());
                    
                    svTrack.MainGroup.Params = EncodeParams(singingTrack.EditedParams);
                    
                    LyricsPinyin = PhonemeUtils.LyricsToPinyin(NoteBuffer.ConvertAll(note =>
                        {
                            if (!string.IsNullOrEmpty(note.Pronunciation))
                            {
                                return note.Pronunciation;
                            }
                            var validChars = Regex.Replace(
                                note.Lyric,
                                "[\\s\\(\\)\\[\\]\\{\\}\\^_*×――—（）$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”0-9a-zA-Z]",
                                "la");
                            return validChars == "" ? "" : validChars[0].ToString();
                        }));
                    svTrack.MainGroup.Notes = EncodeNotesWithPhones(singingTrack.NoteList);

                    // vibrato options
                    switch (VibratoOptions)
                    {
                        case VibratoOptions.None:
                            foreach (var note in svTrack.MainGroup.Notes)
                            {
                                note.Attributes.VibratoDepth = 0.0;
                            }
                            break;
                        case VibratoOptions.Hybrid:
                            foreach (var index in NoVibratoIndexes)
                            {
                                svTrack.MainGroup.Notes[index].Attributes.VibratoDepth = 0.0;
                            }
                            break;
                        case VibratoOptions.Always:
                        default:
                            break;
                    }
                    break;
                case InstrumentalTrack instrumentalTrack:
                    svTrack.MainRef.IsInstrumental = true;
                    svTrack.DispColor = "ff4794cb";
                    svTrack.MainRef.Audio.Filename = instrumentalTrack.AudioFilePath;
                    svTrack.MainRef.BlickOffset = EncodeAudioOffset(instrumentalTrack.Offset);
                    try
                    {
                        using (var reader = new AudioFileReader(instrumentalTrack.AudioFilePath))
                        {
                            svTrack.MainRef.Audio.Duration = reader.TotalTime.TotalSeconds;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    break;
                default:
                    return null;
            }
            return svTrack;
        }

        private double EncodeVolume(double volume)
        {
            return Math.Max(20 * Math.Log10(volume > 0.06 ? volume : 0.06), -24.0);
        }

        private SVParams EncodeParams(Params parameters)
        {
            var svParams = new SVParams
            {
                Pitch = EncodePitchCurve(parameters.Pitch),
                Loudness = EncodeParamCurve(parameters.Volume, 0, 0.0,
                    val => val >= 0
                        ? val / 1000.0 * 12.0
                        : Math.Max(20 * Math.Log10(val > -750.0 ? val / 1000.0 + 1.0 : 0.25), -12.0)),
                Tension = EncodeParamCurve(parameters.Strength, 0, 0.0,
                    val => val / 1000.0),
                Breath = EncodeParamCurve(parameters.Breath, 0, 0.0,
                    val => val / 1000.0),
                Gender = EncodeParamCurve(parameters.Gender, 0, 0.0,
                    val => -val / 1000.0)
            };
            return svParams;
        }

        private SVParamCurve EncodePitchCurve(ParamCurve curve)
        {
            var svCurve = new SVParamCurve();
            if (VibratoOptions == VibratoOptions.Hybrid)
            {
                NoVibratoIndexes = new HashSet<int>();
            }
            if (!NoteBuffer.Any())
            {
                return svCurve;
            }
            var pointList = svCurve.Points;
            var buffer = new List<Tuple<int, int>>();
            const int minInterval = 1;
            Tuple<int, int> lastPoint = null;
            foreach (var point in curve.PointList
                         .Where(point => point.Item1 >= FirstBarTick).ToList()
                         .ConvertAll(point => new Tuple<int, int>(point.Item1 - FirstBarTick, point.Item2)))
            {
                if (point.Item2 == -100)
                {
                    if (!buffer.Any()) continue;
                    if (lastPoint == null || lastPoint.Item1 + minInterval < buffer[0].Item1)
                    {
                        if (lastPoint != null && lastPoint.Item1 + 2 * minInterval < buffer[0].Item1)
                        {
                            pointList.Add(new Tuple<long, double>(EncodePosition(lastPoint.Item1 + minInterval), 0.0));
                        }
                        pointList.Add(new Tuple<long, double>(EncodePosition(buffer[0].Item1 - minInterval), 0.0));
                    }
                    foreach (var (bufferPos, bufferVal) in buffer)
                    {
                        pointList.Add(new Tuple<long, double>(
                            EncodePosition(bufferPos),
                            EncodePitchDiff(bufferPos, bufferVal)));
                    }
                    lastPoint = buffer.Last();
                    buffer.Clear();
                }
                else
                {
                    buffer.Add(point);
                }
            }
            if (lastPoint != null)
            {
                pointList.Add(new Tuple<long, double>(EncodePosition(lastPoint.Item1 + minInterval), 0.0));
            }
            return svCurve;
        }
 
        private double EncodePitchDiff(int pos, int pitch)
        {
            var targetNoteIndex = NoteBuffer.FindLastIndex(note => note.StartPos <= pos);
            var targetNote = targetNoteIndex >= 0 ? NoteBuffer[targetNoteIndex] : null;
            var pitchDiff = pitch - PitchGenerator.PitchAtSecs(Synchronizer.GetActualSecsFromTicks(pos));
            if (targetNote == null) // position before all heads of note
            {
                return pitchDiff;
            }
            // hybrid mode is on, and the point is within the range where the automatic vibrato should have been
            if (VibratoOptions == VibratoOptions.Hybrid
                && Synchronizer.GetDurationSecsFromTicks(targetNote.StartPos, pos) > 0.25
                && pos < targetNote.StartPos + targetNote.Length)
            {
                NoVibratoIndexes.Add(targetNoteIndex);
            }
            return pitchDiff;
        }

        private SVParamCurve EncodeParamCurve(ParamCurve curve, int termination, double defaultValue, Func<int, double> op)
        {
            var svCurve = new SVParamCurve();
            var pointList = svCurve.Points;
            var buffer = new List<Tuple<int, int>>();
            const int minInterval = 1;
            Tuple<int, int> lastPoint = null;
            foreach (var point in curve.PointList
                         .Where(point => point.Item1 >= FirstBarTick).ToList()
                         .ConvertAll(point => new Tuple<int, int>(point.Item1 - FirstBarTick, point.Item2)))
            {
                if (point.Item2 == termination)
                {
                    if (!buffer.Any()) continue;
                    if (lastPoint == null || lastPoint.Item1 + minInterval < buffer[0].Item1)
                    {
                        if (lastPoint != null && lastPoint.Item1 + 2 * minInterval < buffer[0].Item1)
                        {
                            pointList.Add(new Tuple<long, double>(EncodePosition(lastPoint.Item1 + minInterval), defaultValue));
                        }
                        pointList.Add(new Tuple<long, double>(EncodePosition(buffer[0].Item1 - minInterval), defaultValue));
                    }
                    foreach (var (bufferPos, bufferVal) in buffer)
                    {
                        pointList.Add(new Tuple<long, double>(EncodePosition(bufferPos), op(bufferVal)));
                    }
                    lastPoint = buffer.Last();
                    buffer.Clear();
                }
                else
                {
                    buffer.Add(point);
                }
            }
            if (lastPoint != null)
            {
                pointList.Add(new Tuple<long, double>(EncodePosition(lastPoint.Item1 + minInterval), defaultValue));
            }
            return svCurve;
        }

        private List<SVNote> EncodeNotesWithPhones(List<Note> notes)
        {
            var svNoteList = new List<SVNote>();
            if (!notes.Any())
            {
                return svNoteList;
            }

            var currentSVNote = EncodeNote(notes[0]);
            var currentPhoneMarks = PhonemeUtils.DefaultPhoneMarks(LyricsPinyin[0]);
            // head part of the first note
            if (currentPhoneMarks[0] > 0 && notes[0].EditedPhones != null && notes[0].EditedPhones.HeadLengthInSecs > 0)
            {
                var ratio = notes[0].EditedPhones.HeadLengthInSecs / currentPhoneMarks[0];
                currentSVNote.Attributes.SetPhoneDuration(0, ratio < 0.2 ? 0.2 : ratio > 1.8 ? 1.8 : ratio);
            }
            var i = 0;
            for (; i < notes.Count - 1; i++)
            {
                var nextSVNote = EncodeNote(notes[i + 1]);
                var nextPhoneMarks = PhonemeUtils.DefaultPhoneMarks(LyricsPinyin[i + 1]);
                
                var currentMainPartEdited =
                    currentPhoneMarks[1] > 0
                    && notes[i].EditedPhones != null
                    && notes[i].EditedPhones.MidRatioOverTail > 0;
                var nextHeadPartEdited =
                    nextPhoneMarks[0] > 0
                    && notes[i + 1].EditedPhones != null
                    && notes[i + 1].EditedPhones.HeadLengthInSecs > 0;

                if (currentMainPartEdited && nextHeadPartEdited) // three parts should all be adjusted
                {
                    var currentMainRatio = notes[i].EditedPhones.MidRatioOverTail / currentPhoneMarks[1];
                    var nextHeadRatio = notes[i + 1].EditedPhones.HeadLengthInSecs / nextPhoneMarks[0];
                    var x = 2 * currentMainRatio / (1 + currentMainRatio);
                    var y = 2 / (1 + currentMainRatio);
                    var z = nextHeadRatio;
                    if (Synchronizer.GetDurationSecsFromTicks(notes[i].StartPos + notes[i].Length, notes[i + 1].StartPos) < nextPhoneMarks[0])
                    {
                        var finalRatio = 2 / (1 + nextHeadRatio);
                        x *= finalRatio;
                        y *= finalRatio;
                        z *= finalRatio;
                    }
                    currentSVNote.Attributes.SetPhoneDuration(1, x < 0.2 ? 0.2 : x > 1.8 ? 1.8 : x);
                    currentSVNote.Attributes.SetPhoneDuration(2, y < 0.2 ? 0.2 : y > 1.8 ? 1.8 : y);
                    nextSVNote.Attributes.SetPhoneDuration(0, z < 0.2 ? 0.2 : z > 1.8 ? 1.8 : z);
                }
                else if (currentMainPartEdited) // only main part of current note should be adjusted
                {
                    var ratio = notes[i].EditedPhones.MidRatioOverTail / currentPhoneMarks[1];
                    var x = 2 * ratio / (1 + ratio);
                    var y = 2 / (1 + ratio);
                    currentSVNote.Attributes.SetPhoneDuration(1, x < 0.2 ? 0.2 : x > 1.8 ? 1.8 : x);
                    currentSVNote.Attributes.SetPhoneDuration(2, y < 0.2 ? 0.2 : y > 1.8 ? 1.8 : y);
                }
                else if (nextHeadPartEdited) // only head part of next note should be adjusted
                {
                    var ratio = notes[i + 1].EditedPhones.HeadLengthInSecs / nextPhoneMarks[0];
                    if (Synchronizer.GetDurationSecsFromTicks(notes[i].StartPos + notes[i].Length, notes[i + 1].StartPos) < nextPhoneMarks[0])
                    {
                        var ratioZ = 2 * ratio / (1 + ratio);
                        var ratioXY = 2 / (1 + ratio);
                        ratioZ = ratioZ < 0.2 ? 0.2 : ratioZ > 1.8 ? 1.8 : ratioZ;
                        ratioXY = ratioXY < 0.2 ? 0.2 : ratioXY > 1.8 ? 1.8 : ratioXY;
                        currentSVNote.Attributes.SetPhoneDuration(1, ratioXY);
                        if (currentPhoneMarks[1] > 0.0)
                        {
                            currentSVNote.Attributes.SetPhoneDuration(2, ratioXY);
                        }
                        ratio = ratioZ;
                    }
                    nextSVNote.Attributes.SetPhoneDuration(0, ratio);
                }
                // check length of phone duration array if edited
                if (currentSVNote.Attributes.PhoneDurations != null)
                {
                    var expectedLength = PhonemeUtils.NumberOfPhones(LyricsPinyin[i]);
                    if (currentSVNote.Attributes.PhoneDurations.Length < expectedLength)
                    {
                        currentSVNote.Attributes.SetPhoneDuration(expectedLength - 1, 1.0);
                    }
                }
                svNoteList.Add(currentSVNote);
                currentSVNote = nextSVNote;
                currentPhoneMarks = nextPhoneMarks;
            }
            // main part of the last note
            if (currentPhoneMarks[1] > 0 && notes[i].EditedPhones != null && notes[i].EditedPhones.MidRatioOverTail > 0)
            {
                var ratio = notes[i].EditedPhones.MidRatioOverTail / currentPhoneMarks[1];
                var x = 2 * ratio / (1 + ratio);
                var y = 2 / (1 + ratio);
                currentSVNote.Attributes.SetPhoneDuration(1, x < 0.2 ? 0.2 : x > 1.8 ? 1.8 : x);
                currentSVNote.Attributes.SetPhoneDuration(2, y < 0.2 ? 0.2 : y > 1.8 ? 1.8 : y);
            }
            // check length of phone duration array if edited
            if (currentSVNote.Attributes.PhoneDurations != null)
            {
                var expectedLength = PhonemeUtils.NumberOfPhones(LyricsPinyin[i]);
                if (currentSVNote.Attributes.PhoneDurations.Length < expectedLength)
                {
                    currentSVNote.Attributes.SetPhoneDuration(expectedLength - 1, 1.0);
                }
            }
            svNoteList.Add(currentSVNote);
            return svNoteList;
        }

        private SVNote EncodeNote(Note note)
        {
            var svNote = new SVNote
            {
                Onset = EncodePosition(note.StartPos),
                Pitch = note.KeyNumber,
                Lyrics = note.Pronunciation ?? note.Lyric
            };
            svNote.Duration = EncodePosition(note.StartPos + note.Length) - svNote.Onset;
            return svNote;
        }

        private long EncodeAudioOffset(int offset)
        {
            if (offset >= 0)
            {
                return EncodePosition(offset);
            }
            var currentPos = FirstBarTick;
            var actualPos = FirstBarTick + offset;
            var res = 0.0;
            var i = FirstBarTempo.Count - 1;
            for (; i >= 0 && actualPos <= FirstBarTempo[i].Position; i--)
            {
                res -= (currentPos - FirstBarTempo[i].Position) * 120 / FirstBarTempo[i].BPM;
                currentPos = FirstBarTempo[i].Position;
            }
            if (i >= 0)
            {
                res -= (currentPos - actualPos) * 120 / FirstBarTempo[i].BPM;
            }
            else
            {
                res += actualPos * 120 / FirstBarTempo[0].BPM;
            }
            return (long) Math.Round(res * 1470000L);
        }

        private long EncodePosition(int position)
        {
            return (long) Math.Round(Synchronizer.GetActualTicksFromTicks(position) * 1470000L);
        }
    }
}
