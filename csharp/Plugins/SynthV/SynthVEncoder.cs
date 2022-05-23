using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NAudio.Wave;
using OpenSvip.Model;
using OpenSvip.Library;
using SynthV.Model;
using SynthV.Options;
using SynthV.Utils;
using SynthV.Param;

namespace SynthV.Core
{
    public class SynthVEncoder
    {
        public VibratoOptions VibratoOption { get; set; } = VibratoOptions.Hybrid;

        public int ParamSampleInterval { get; set; }

        private int FirstBarTick;

        private List<SongTempo> FirstBarTempo;

        private List<Note> NoteBuffer;

        private TimeSynchronizer Synchronizer;

        private PitchSimulator PitchSimulator;

        private HashSet<int> NoVibratoIndexes;

        private List<string> LyricsPinyin;

        public SVProject EncodeProject(Project project)
        {
            var svProject = new SVProject();
            var newMeters = ScoreMarkUtils.SkipBeatList(project.TimeSignatureList, 1);
            FirstBarTick = (int) Math.Round(1920.0 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator);
            FirstBarTempo = project.SongTempoList.Where(tempo => tempo.Position < FirstBarTick).ToList();
            Synchronizer = new TimeSynchronizer(project.SongTempoList, FirstBarTick);
            foreach (var tempo in Synchronizer.TempoList)
            {
                svProject.Time.Tempos.Add(EncodeTempo(tempo));
            }
            if (project.TimeSignatureList.Any(beat => beat.Denominator < 2 || beat.Denominator > 16))
            {
                svProject.Time.Meters.Add(new SVMeter
                {
                    Index = 0,
                    Numerator = 4,
                    Denominator = 4
                });
            }
            else
            {
                foreach (var meter in newMeters)
                {
                    svProject.Time.Meters.Add(EncodeMeter(meter));
                }
            }
            var id = 0;
            foreach (var svTrack in project.TrackList.Select(EncodeTrack))
            {
                svTrack.DisplayOrder = id;
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
                Position = TicksToPosition(tempo.Position),
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
                    svTrack.DisplayColor = "ff7db235";
                    svTrack.MainRef.Database.Language = "mandarin";
                    svTrack.MainRef.Database.PhoneSet = "xsampa";
                    
                    NoteBuffer = singingTrack.NoteList;
                    PitchSimulator = new PitchSimulator(Synchronizer,
                        singingTrack.NoteList, PitchSlide.SigmoidSlide());
                    
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
                    switch (VibratoOption)
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
                    svTrack.DisplayColor = "ff4794cb";
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
                Pitch = EncodePitchCurve(parameters.Pitch.ReduceSampleRate(ParamSampleInterval, -100)),
                Loudness = EncodeParamCurve(parameters.Volume.ReduceSampleRate(ParamSampleInterval),
                    0, 0.0,
                    val => val >= 0
                        ? val / 1000.0 * 12.0
                        : Math.Max(20 * Math.Log10(val > -997 ? val / 1000.0 + 1.0 : 0.0039), -48.0)),
                Tension = EncodeParamCurve(parameters.Strength.ReduceSampleRate(ParamSampleInterval),
                    0, 0.0,
                    val => val / 1000.0),
                Breath = EncodeParamCurve(parameters.Breath.ReduceSampleRate(ParamSampleInterval),
                    0, 0.0,
                    val => val / 1000.0),
                Gender = EncodeParamCurve(parameters.Gender.ReduceSampleRate(ParamSampleInterval),
                    0, 0.0,
                    val => -val / 1000.0)
            };
            return svParams;
        }

        private SVParamCurve EncodePitchCurve(ParamCurve curve)
        {
            var svCurve = new SVParamCurve();
            if (VibratoOption == VibratoOptions.Hybrid)
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
                            pointList.Add(new Tuple<long, double>(TicksToPosition(lastPoint.Item1 + minInterval), 0.0));
                        }
                        pointList.Add(new Tuple<long, double>(TicksToPosition(buffer[0].Item1 - minInterval), 0.0));
                    }
                    foreach (var (bufferPos, bufferVal) in buffer)
                    {
                        pointList.Add(new Tuple<long, double>(
                            TicksToPosition(bufferPos),
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
                pointList.Add(new Tuple<long, double>(TicksToPosition(lastPoint.Item1 + minInterval), 0.0));
            }
            return svCurve;
        }
 
        private double EncodePitchDiff(int pos, int pitch)
        {
            var targetNoteIndex = NoteBuffer.FindLastIndex(note => note.StartPos <= pos);
            var targetNote = targetNoteIndex >= 0 ? NoteBuffer[targetNoteIndex] : null;
            var pitchDiff = pitch - PitchSimulator.PitchAtSecs(Synchronizer.GetActualSecsFromTicks(pos));
            if (targetNote == null) // position before all heads of note
            {
                return pitchDiff;
            }
            // hybrid mode is on, and the point is within the range where the automatic vibrato should have been
            if (VibratoOption == VibratoOptions.Hybrid
                && Synchronizer.GetDurationSecsFromTicks(targetNote.StartPos, pos) > 0.25
                && pos < targetNote.StartPos + targetNote.Length)
            {
                NoVibratoIndexes.Add(targetNoteIndex);
            }
            return pitchDiff;
        }

        private SVParamCurve EncodeParamCurve(ParamCurve curve, int termination, double defaultValue, Func<int, double> mappingFunc)
        {
            var svCurve = new SVParamCurve();
            if (!curve.PointList.Any())
            {
                return svCurve;
            }

            if (ParamSampleInterval > 15)
            {
                svCurve.Mode = "cubic";
            }
            var pointList = svCurve.Points;
            var skipped = 0;
            if (curve.PointList[0].Item1 == -192000)
            {
                if (curve.PointList.Count == 2 && curve.PointList[1].Item1 == 1073741823)
                {
                    if (curve.PointList[0].Item2 != termination)
                    {
                        pointList.Add(new Tuple<long, double>(0, mappingFunc(curve.PointList[0].Item2)));
                    }
                    return svCurve;
                }
                skipped = 1;
                var validIndex = curve.PointList.FindIndex(point => point.Item1 >= FirstBarTick);
                if (validIndex != -1 && curve.PointList.Count > validIndex + 1
                                     && !(curve.PointList[validIndex].Item2 == termination
                                          && curve.PointList[validIndex + 1].Item2 == termination
                                          && curve.PointList[validIndex + 1].Item1 < 1073741823))
                {
                    skipped = validIndex + 1;
                    var (x0, y0) = curve.PointList[validIndex];
                    pointList.Add(new Tuple<long, double>(TicksToPosition(x0 - FirstBarTick), mappingFunc(y0)));
                }
            }
            var buffer = new List<Tuple<int, int>>();
            const int minInterval = 1;
            Tuple<int, int> lastPoint = null;
            foreach (var point in curve.PointList
                         .Skip(skipped)
                         .Where(point => point.Item1 >= FirstBarTick && point.Item1 < 1073741823).ToList()
                         .ConvertAll(point => new Tuple<int, int>(point.Item1 - FirstBarTick, point.Item2)))
            {
                if (point.Item2 == termination)
                {
                    if (!buffer.Any()) continue;
                    if (lastPoint == null || lastPoint.Item1 + minInterval < buffer[0].Item1)
                    {
                        if (lastPoint != null && lastPoint.Item1 + 2 * minInterval < buffer[0].Item1)
                        {
                            pointList.Add(new Tuple<long, double>(TicksToPosition(lastPoint.Item1 + minInterval), defaultValue));
                        }
                        pointList.Add(new Tuple<long, double>(TicksToPosition(buffer[0].Item1 - minInterval), defaultValue));
                    }
                    foreach (var (bufferPos, bufferVal) in buffer)
                    {
                        pointList.Add(new Tuple<long, double>(TicksToPosition(bufferPos), mappingFunc(bufferVal)));
                    }
                    lastPoint = buffer.Last();
                    buffer.Clear();
                }
                else
                {
                    buffer.Add(point);
                }
            }
            if (!buffer.Any())
            {
                if (lastPoint != null)
                {
                    pointList.Add(new Tuple<long, double>(TicksToPosition(lastPoint.Item1 + minInterval), defaultValue));
                }
                return svCurve;
            }
            if (lastPoint == null || lastPoint.Item1 + minInterval < buffer[0].Item1)
            {
                if (lastPoint != null && lastPoint.Item1 + 2 * minInterval < buffer[0].Item1)
                {
                    pointList.Add(new Tuple<long, double>(TicksToPosition(lastPoint.Item1 + minInterval), defaultValue));
                }
                pointList.Add(new Tuple<long, double>(TicksToPosition(buffer[0].Item1 - minInterval), defaultValue));
            }
            foreach (var (bufferPos, bufferVal) in buffer)
            {
                pointList.Add(new Tuple<long, double>(TicksToPosition(bufferPos), mappingFunc(bufferVal)));
            }
            lastPoint = buffer.Last();
            buffer.Clear();
            if (lastPoint.Item2 == termination)
            {
                pointList.Add(new Tuple<long, double>(TicksToPosition(lastPoint.Item1 + minInterval), defaultValue));
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

                var index = currentPhoneMarks[0] > 0 ? 1 : 0;
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
                    currentSVNote.Attributes.SetPhoneDuration(index, x < 0.2 ? 0.2 : x > 1.8 ? 1.8 : x);
                    currentSVNote.Attributes.SetPhoneDuration(index + 1, y < 0.2 ? 0.2 : y > 1.8 ? 1.8 : y);
                    nextSVNote.Attributes.SetPhoneDuration(0, z < 0.2 ? 0.2 : z > 1.8 ? 1.8 : z);
                }
                else if (currentMainPartEdited) // only main part of current note should be adjusted
                {
                    var ratio = notes[i].EditedPhones.MidRatioOverTail / currentPhoneMarks[1];
                    var x = 2 * ratio / (1 + ratio);
                    var y = 2 / (1 + ratio);
                    currentSVNote.Attributes.SetPhoneDuration(index, x < 0.2 ? 0.2 : x > 1.8 ? 1.8 : x);
                    currentSVNote.Attributes.SetPhoneDuration(index + 1, y < 0.2 ? 0.2 : y > 1.8 ? 1.8 : y);
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
                        currentSVNote.Attributes.SetPhoneDuration(index, ratioXY);
                        if (currentPhoneMarks[1] > 0.0)
                        {
                            currentSVNote.Attributes.SetPhoneDuration(index + 1, ratioXY);
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
                var index = currentPhoneMarks[0] > 0 ? 1 : 0;
                currentSVNote.Attributes.SetPhoneDuration(index, x < 0.2 ? 0.2 : x > 1.8 ? 1.8 : x);
                currentSVNote.Attributes.SetPhoneDuration(index + 1, y < 0.2 ? 0.2 : y > 1.8 ? 1.8 : y);
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
                Onset = TicksToPosition(note.StartPos),
                Pitch = note.KeyNumber,
                Lyrics = string.IsNullOrWhiteSpace(note.Pronunciation) ? note.Lyric : note.Pronunciation
            };
            svNote.Duration = TicksToPosition(note.StartPos + note.Length) - svNote.Onset;
            return svNote;
        }

        private long EncodeAudioOffset(int offset)
        {
            if (offset >= 0)
            {
                return TicksToPosition(offset);
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

        private static long TicksToPosition(int position)
        {
            return (long) Math.Round((double) position * 1470000L);
        }
    }
}
