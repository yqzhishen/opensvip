using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NAudio.Wave;
using OpenSvip.Model;
using SynthV.Model;

namespace Plugin.SynthV
{
    public class SynthVEncoder
    {
        public int DefaultTempo { get; set; } = 60;

        public VibratoOptions VibratoOptions { get; set; } = VibratoOptions.Hybrid;
        
        private bool IsAbsoluteTimeMode;

        private int FirstBarTick;

        private List<SongTempo> FirstBarTempo;

        private List<SongTempo> TempoBuffer;

        private List<Note> NoteBuffer;

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
            FirstBarTick = 1920 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator;
            FirstBarTempo = project.SongTempoList.Where(tempo => tempo.Position < FirstBarTick).ToList();
            var newTempos = project.SongTempoList
                .Where(tempo => tempo.Position >= FirstBarTick)
                .Select(
                    tempo => new SongTempo
                    {
                        Position = tempo.Position - FirstBarTick,
                        BPM = tempo.BPM
                    }).ToList();
            if (!newTempos.Any() || newTempos[0].Position > 0)
            {
                var i = 0;
                for (; i < project.SongTempoList.Count && project.SongTempoList[i].Position <= FirstBarTick; i++) { }
                newTempos.Insert(0, new SongTempo
                {
                    Position = 0,
                    BPM = project.SongTempoList[i - 1].BPM
                });
            }
            TempoBuffer = newTempos;
            if (newMeters.Any(meter => meter.Denominator < 2 || meter.Denominator > 16))
            {
                IsAbsoluteTimeMode = true;
            }
            if (!IsAbsoluteTimeMode)
            {
                foreach (var meter in newMeters)
                {
                    svProject.Time.Meters.Add(EncodeMeter(meter));
                }
                foreach (var tempo in newTempos)
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
                        pointList.Add(new Tuple<long, double>(EncodePosition(bufferPos), EncodePitchDiff(bufferPos, bufferVal)));
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
            var maxSlideTimeInSecs = 0.05;
            var defaultSlidePercent = 0.1;
            var targetNoteIndex = NoteBuffer.FindLastIndex(note => note.StartPos <= pos);
            var targetNote = targetNoteIndex >= 0 ? NoteBuffer[targetNoteIndex] : null;
            var nextNote = targetNoteIndex < NoteBuffer.Count - 1 ? NoteBuffer[targetNoteIndex + 1] : null;
            var prevNote = targetNoteIndex > 0 ? NoteBuffer[targetNoteIndex - 1] : null;
            if (targetNote == null) // position before all heads of note
            {
                return pitch - NoteBuffer[0].KeyNumber * 100;
            }
            // hybrid mode is on, and the point is within the range where the automatic vibrato should have been
            if (VibratoOptions == VibratoOptions.Hybrid && DurationPositionToSecs(targetNote.StartPos, pos) > 0.25)
            {
                NoVibratoIndexes.Add(targetNoteIndex);
            }
            if (nextNote == null) // position after all heads of note
            {
                return pitch - NoteBuffer.Last().KeyNumber * 100;
            }
            // position is between note heads
            var timeAfterHeadInSecs = DurationPositionToSecs(targetNote.StartPos, pos);
            var headSlidePartInSecs = Math.Min(maxSlideTimeInSecs, defaultSlidePercent * DurationPositionToSecs(
                targetNote.StartPos, targetNote.StartPos + targetNote.Length));
            if (timeAfterHeadInSecs < headSlidePartInSecs) // position is near the head of target note
            {
                if (prevNote == null) // there is no previous note (position is only after head of the first note)
                {
                    return pitch - targetNote.KeyNumber * 100;
                }
                var prevSlidePartInSecs = Math.Min(maxSlideTimeInSecs, defaultSlidePercent * DurationPositionToSecs(
                    prevNote.StartPos,
                    prevNote.StartPos + prevNote.Length));
                var intervalPartInSecs = DurationPositionToSecs(prevNote.StartPos + prevNote.Length, targetNote.StartPos);
                var interpolationSecs = prevSlidePartInSecs + intervalPartInSecs + headSlidePartInSecs;
                if (interpolationSecs > maxSlideTimeInSecs * 2) // previous note is too far away
                {
                    return pitch - targetNote.KeyNumber * 100;
                }
                // cosine interpolation of the slide part
                var ratio = 0.5 * (1 + Math.Cos(
                    Math.PI * (prevSlidePartInSecs + intervalPartInSecs + timeAfterHeadInSecs) / interpolationSecs));
                return pitch - (ratio * prevNote.KeyNumber + (1 - ratio) * targetNote.KeyNumber) * 100;
            }
            var noteLengthInSecs = DurationPositionToSecs(targetNote.StartPos, targetNote.StartPos + targetNote.Length);
            var noSlidePartInSecs = noteLengthInSecs - Math.Min(maxSlideTimeInSecs, defaultSlidePercent * noteLengthInSecs);
            if (timeAfterHeadInSecs > noSlidePartInSecs) // position is near the tail of target note
            {
                var tailSlidePartInSecs = noteLengthInSecs - noSlidePartInSecs;
                var intervalPartInSecs =
                    DurationPositionToSecs(targetNote.StartPos + targetNote.Length, nextNote.StartPos);
                var nextSlidePartInSecs = Math.Min(maxSlideTimeInSecs, defaultSlidePercent * DurationPositionToSecs(
                    nextNote.StartPos, nextNote.StartPos + nextNote.Length));
                var interpolationSecs = tailSlidePartInSecs + intervalPartInSecs + nextSlidePartInSecs;
                if (interpolationSecs > maxSlideTimeInSecs * 2) // next note is too far away
                {
                    if (timeAfterHeadInSecs > noteLengthInSecs) // position between tail of target note and head of next note
                    {
                        return timeAfterHeadInSecs - noteLengthInSecs < interpolationSecs / 2
                            ? pitch - targetNote.KeyNumber * 100 // closer to the tail of target note
                            : pitch - nextNote.KeyNumber * 100; // closer to the head of next note
                    }
                    // before tail of target note
                    return pitch - targetNote.KeyNumber * 100;
                }
                // cosine interpolation of the slide part
                var ratio = 0.5 * (1 + Math.Cos(
                    Math.PI * (timeAfterHeadInSecs - noSlidePartInSecs) / interpolationSecs));
                return pitch - (ratio * targetNote.KeyNumber + (1 - ratio) * nextNote.KeyNumber) * 100;
            }
            // position is within the middle of target note
            return pitch - targetNote.KeyNumber * 100;
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
                    if (DurationPositionToSecs(notes[i].StartPos + notes[i].Length, notes[i + 1].StartPos) < nextPhoneMarks[0])
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
                    if (DurationPositionToSecs(notes[i].StartPos + notes[i].Length, notes[i + 1].StartPos) < nextPhoneMarks[0])
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
                Onset = EncodePosition(note.StartPos)
            };
            svNote.Duration = EncodePosition(note.StartPos + note.Length) - svNote.Onset;
            svNote.Pitch = note.KeyNumber;
            svNote.Lyrics = note.Pronunciation ?? note.Lyric;
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
            if (!IsAbsoluteTimeMode)
            {
                return position * 1470000L;
            }
            var res = 0.0;
            var i = 0;
            for (; i < TempoBuffer.Count - 1 && TempoBuffer[i + 1].Position < position; i++)
            {
                res += (TempoBuffer[i + 1].Position - TempoBuffer[i].Position) * DefaultTempo / TempoBuffer[i].BPM;
            }
            res += (position - TempoBuffer[i].Position) * DefaultTempo / TempoBuffer[i].BPM;
            return (long) Math.Round(res * 1470000L);
        }

        private double DurationPositionToSecs(int startPos, int endPos)
        {
            double PositionToTick(int position)
            {
                var tick = 0.0;
                var i = 0;
                for (; i < TempoBuffer.Count - 1 && TempoBuffer[i + 1].Position < position; i++)
                {
                    tick += (TempoBuffer[i + 1].Position - TempoBuffer[i].Position) * (double) DefaultTempo / TempoBuffer[i].BPM;
                }
                tick += (position - TempoBuffer[i].Position) * DefaultTempo / (double) TempoBuffer[i].BPM;
                return tick;
            }

            if (IsAbsoluteTimeMode)
            {
                return (PositionToTick(endPos) - PositionToTick(startPos)) / DefaultTempo / 8;
            }
            
            var startTempoIndex = TempoBuffer.FindLastIndex(tempo => tempo.Position <= startPos);
            var endTempoIndex = TempoBuffer.FindIndex(tempo => tempo.Position >= endPos);
            
            if (endTempoIndex == -1 || startTempoIndex + 1 == endTempoIndex)
            {
                return (endPos - startPos) / TempoBuffer[startTempoIndex].BPM / 8;
            }
            
            var secs = 0.0;
            secs += (TempoBuffer[startTempoIndex + 1].Position - startPos)
                        / (double) TempoBuffer[startTempoIndex].BPM / 8;
            for (var i = startTempoIndex + 1; i < endTempoIndex - 1; i++)
            {
                secs += (TempoBuffer[i + 1].Position - TempoBuffer[i].Position) / (double) TempoBuffer[i].BPM / 8;
            }
            secs += (endPos - TempoBuffer[endTempoIndex - 1].Position) / (double) TempoBuffer[endTempoIndex - 1].BPM / 8;
            return secs;
        }
    }
}
