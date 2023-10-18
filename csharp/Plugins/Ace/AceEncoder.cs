using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AceStdio.Model;
using AceStdio.Options;
using AceStdio.Param;
using AceStdio.Resources;
using AceStdio.Utils;
using NAudio.Wave;
using OpenSvip.Library;
using OpenSvip.Model;

namespace AceStdio.Core
{
    public class AceEncoder
    {
        public string DefaultSinger { get; set; }
        
        public double DefaultTempo { get; set; }
        
        public int LagCompensation { get; set; }

        public int BreathLength { get; set; }
        
        public StrengthMappingOption MapStrengthTo { get; set; }
        
        public int SplitThreshold { get; set; }

        private TimeSynchronizer _synchronizer;

        private int _firstBarTicks;

        private List<SongTempo> _firstBarTempo;

        private bool _hasMultiTempo;

        private List<AceTempo> _aceTempoList;

        private List<AceNote> _noteList;

        private int _patternStart;

        public AceContent EncodeProject(Project project)
        {
            var content = new AceContent();
            _firstBarTicks = (int)Math.Round(1920.0 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator);
            _firstBarTempo = project.SongTempoList.Where(tempo => tempo.Position < _firstBarTicks).ToList();
            var denominator = project.TimeSignatureList[0].Denominator;
            var numerator = project.TimeSignatureList[0].Numerator * 4 / denominator;
            switch (numerator)
            {
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 8:
                    content.BeatsPerBar = numerator;
                    break;
            }

            var shiftedTempo = ScoreMarkUtils.ShiftTempoList(project.SongTempoList, _firstBarTicks);
            if (shiftedTempo.Count == 1 || shiftedTempo.GroupBy(tempo => tempo.BPM).Count() == 1)
            {
                _hasMultiTempo = false;
                DefaultTempo = project.SongTempoList[0].BPM;
            }
            else
            {
                _hasMultiTempo = true;
            }
            content.TempoList.Add(new AceTempo
            {
                Position = 0,
                BPM = DefaultTempo
            });
            _aceTempoList = content.TempoList;
            
            _synchronizer = new TimeSynchronizer(project.SongTempoList, _firstBarTicks, _hasMultiTempo)
            {
                DefaultTempo = DefaultTempo
            };
            
            content.TrackList.AddRange(project.TrackList.Select(EncodeTrack).Where(track => track != null));
            content.Duration = content.TrackList.Any()
                ? content.TrackList.Max(track => track.Length()) + 9600
                : 9600;
            var colorCount = ColorPool.CountColor();
            var colorIndex = new Random().Next(colorCount);
            for (var i = 0; i < content.TrackList.Count; ++i, ++colorIndex)
            {
                content.TrackList[i].Color = ColorPool.GetColor(colorIndex);
            }
            content.ColorIndex = colorIndex % colorCount;
            return content;
        }

        private AceTrack EncodeTrack(Track track)
        {
            AceTrack aceTrack;
            switch (track)
            {
                case SingingTrack singingTrack:
                    var aceVocalTrack = new AceVocalTrack
                    {
                        // Singer = AceSingers.GetId(singingTrack.AISingerName) > 0 ? singingTrack.AISingerName : DefaultSinger
                    };

                    if (!singingTrack.NoteList.Any())
                    {
                        goto done;
                    }

                    var buffer = new List<Note>
                    {
                        singingTrack.NoteList[0]
                    };
                    
                    void EncodeVocalPattern()
                    {
                        var padding = SplitThreshold > 0 ? SplitThreshold / 2 : 1920;
                        _patternStart = (int) Math.Round(
                            _synchronizer.GetActualTicksFromTicks(Math.Max(0, buffer.First().StartPos - padding)));
                        var vocalPattern = new AceVocalPattern
                        {
                            Position = _patternStart,
                            Duration = (int) Math.Round(
                                _synchronizer.GetActualTicksFromTicks(
                                    buffer.Last().StartPos + buffer.Last().Length + padding)) - _patternStart
                        };
                        var pinyinSeries = PinyinUtils.GetPinyinSeries(buffer.Select(note => note.Lyric));
                        vocalPattern.NoteList = buffer.Zip(pinyinSeries, EncodeNote).ToList();
                        vocalPattern.ClipDuration = vocalPattern.Duration;
                        buffer.Clear();
                        
                        if (LagCompensation > 0)
                        {
                            ShiftVowelNotes(vocalPattern.NoteList);
                        }
                        if (BreathLength > 0)
                        {
                            AdjustBreathTags(vocalPattern.NoteList);
                        }
                    
                        _noteList = vocalPattern.NoteList;
                        vocalPattern.Params = EncodeParams(singingTrack.EditedParams);
                        
                        aceVocalTrack.PatternList.Add(vocalPattern);
                    }
                    
                    for (var i = 1; i < singingTrack.NoteList.Count; ++i)
                    {
                        var prevEnd = singingTrack.NoteList[i - 1].StartPos + singingTrack.NoteList[i - 1].Length;
                        var curStart = singingTrack.NoteList[i].StartPos;
                        if (curStart - prevEnd > SplitThreshold && SplitThreshold > 0)
                        {
                            EncodeVocalPattern();
                        }
                        buffer.Add(singingTrack.NoteList[i]);
                    }
                    if (buffer.Any())
                    {
                        EncodeVocalPattern();
                    }
                    
                    done:
                    aceTrack = aceVocalTrack;
                    break;
                case InstrumentalTrack instrumentalTrack:
                    var aceAudioTrack = new AceAudioTrack();
                    var audioPattern = new AceAudioPattern
                    {
                        AudioFilePath = instrumentalTrack.AudioFilePath
                    };
                    
                    var actualOriginalOffset = EncodeAudioOffset(instrumentalTrack.Offset);
                    if (actualOriginalOffset < 0)
                    {
                        audioPattern.ClipPosition = (int)Math.Round(_synchronizer.GetActualTicksFromTicks(-actualOriginalOffset));
                        audioPattern.Position = -audioPattern.ClipPosition;
                    }
                    else
                    {
                        audioPattern.Position = (int)Math.Round(_synchronizer.GetActualTicksFromTicks(actualOriginalOffset));
                    }
                    try
                    {
                        using (var reader = new AudioFileReader(instrumentalTrack.AudioFilePath))
                        {
                            var offset = audioPattern.Position >= 0
                                ? _synchronizer.GetActualSecsFromTicks(audioPattern.Position)
                                : audioPattern.Position / DefaultTempo / 8;
                            audioPattern.Duration = (int) _synchronizer.GetActualTicksFromSecs(offset + reader.TotalTime.TotalSeconds)
                                - audioPattern.Position;
                            audioPattern.ClipDuration = audioPattern.Duration - audioPattern.ClipPosition;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    aceAudioTrack.PatternList.Add(audioPattern);
                    aceTrack = aceAudioTrack;
                    break;
                default:
                    throw new ArgumentException(nameof(track));
            }
            aceTrack.Name = track.Title;
            aceTrack.Mute = track.Mute;
            aceTrack.Solo = track.Solo;
            aceTrack.Gain = Math.Min(6.0, 20 * Math.Log10(track.Volume));
            aceTrack.Pan = track.Pan;
            return aceTrack;
        }

        private AceNote EncodeNote(Note note, string pinyin)
        {
            var aceNote = new AceNote
            {
                Position = (int) Math.Round(_synchronizer.GetActualTicksFromTicks(note.StartPos)),
                Pitch = note.KeyNumber,
                Lyrics = note.Lyric
            };
            aceNote.Duration = (int) Math.Round(_synchronizer.GetActualTicksFromTicks(note.StartPos + note.Length)) - aceNote.Position;
            aceNote.Position -= _patternStart;
            
            if (!note.Lyric.Contains("-"))
            {
                // aceNote.Pronunciation = note.Pronunciation ?? pinyin;
            }
            else
            {
                aceNote.Lyrics = "-";
                // aceNote.Pronunciation = "-";
            }
            if (note.EditedPhones != null && note.EditedPhones.HeadLengthInSecs >= 0)
            {
                var phoneStartInSecs = _synchronizer.GetActualSecsFromTicks(note.StartPos) - note.EditedPhones.HeadLengthInSecs;
                var phoneStartInTicks = _synchronizer.GetActualTicksFromSecs(phoneStartInSecs);
                // aceNote.ConsonantLength = (int)Math.Round(_synchronizer.GetActualTicksFromTicks(note.StartPos) - phoneStartInTicks);
            }
            if (note.HeadTag == "V" && BreathLength > 0)
            {
                var breathStartInSecs = _synchronizer.GetActualSecsFromTicks(note.StartPos) - BreathLength / 1000.0;
                var breathStartInTicks = _synchronizer.GetActualTicksFromSecs(breathStartInSecs);
                aceNote.BreathLength = (int)Math.Round(_synchronizer.GetActualTicksFromTicks(note.StartPos) - breathStartInTicks);
            }
            return aceNote;
        }

        private int GetActualLagCompensation(AceNote note)
        {
            // if (Regex.IsMatch(note.Pronunciation, "^[yw].*"))
            // {
            //     return note.ConsonantLength ?? LagCompensation;
            // }
            //
            // if (Regex.IsMatch(note.Pronunciation, "^[aoe]|(ri).*"))
            // {
            //     return note.ConsonantLength ?? LagCompensation / 2;
            // }
            return 0;
        }

        private void ShiftVowelNotes(List<AceNote> noteList)
        {
            var firstShift = GetActualLagCompensation(noteList[0]);
            if (firstShift > 0)
            {
                var actualShift = Math.Min(noteList[0].Position, firstShift);
                noteList[0].Position -= actualShift;
                noteList[0].Duration += actualShift;
                // noteList[0].ConsonantLength = null;
            }
            
            for (var i = 1; i < noteList.Count; ++i)
            {
                var shift = GetActualLagCompensation(noteList[i]);
                if (shift == 0) continue;
                var actualShift = Math.Min(noteList[i].Position - noteList[i-1].Position - noteList[i-1].Duration / 2, shift);
                noteList[i-1].Duration = Math.Min(noteList[i-1].Duration,
                    noteList[i].Position - actualShift - noteList[i-1].Position);
                noteList[i].Position -= actualShift;
                noteList[i].Duration += actualShift;
                // noteList[i].ConsonantLength = null;
            }
        }

        private void AdjustBreathTags(List<AceNote> noteList)
        {
            for (var i = 1; i < noteList.Count; ++i)
            {
                var breath = noteList[i].BreathLength;
                if (breath == 0) continue;
                var actualBreath = Math.Min((noteList[i].Position - noteList[i-1].Position) / 2, breath);
                noteList[i-1].Duration = Math.Min(noteList[i-1].Duration,
                    noteList[i].Position - actualBreath - noteList[i-1].Position);
                noteList[i].BreathLength = actualBreath;
            }
        }

        private AceParams EncodeParams(Params parameters)
        {
            Func<double, double> LinearTransform(double lowerBound, double middleValue, double upperBound)
            {
                return x => x >= 0
                    ? x * (upperBound - middleValue) / 1000.0 + middleValue
                    : x * (middleValue - lowerBound) / 1000.0 + middleValue;
            }
            var result = new AceParams
            {
                PitchDelta = EncodePitchCurves(parameters.Pitch),
                Breath = EncodeParamCurves(parameters.Breath, LinearTransform(0.2, 1, 2.5)),
                Gender = EncodeParamCurves(parameters.Gender, LinearTransform(-1, 0, 1))
            };
            switch (MapStrengthTo)
            {
                case StrengthMappingOption.Both:
                    result.Energy = EncodeParamCurves(parameters.Strength, x => LinearTransform(0, 1, 2)(x / 2.0));
                    result.Tension = EncodeParamCurves(parameters.Strength, x => LinearTransform(0.7, 1, 1.5)(x / 2.0));
                    break;
                case StrengthMappingOption.Energy:
                    result.Energy = EncodeParamCurves(parameters.Strength, LinearTransform(0, 1, 2));
                    break;
                case StrengthMappingOption.Tension:
                    result.Tension = EncodeParamCurves(parameters.Strength, LinearTransform(0.7, 1, 1.5));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(MapStrengthTo));
            }
            return result;
        }

        private List<AceParamCurve> EncodePitchCurves(ParamCurve curve)
        {
            var aceCurves = new List<AceParamCurve>();
            var basePitch = new BasePitchCurve(_noteList, _aceTempoList, _patternStart);
            var patternStartSecond = TimeUtils.TickToSecond(_patternStart, _aceTempoList);
            var leftBound = TimeUtils.TickToSecond(
                                Math.Max(0, _patternStart + _noteList.First().Position - 240), _aceTempoList)
                            - patternStartSecond;
            var rightBound = TimeUtils.TickToSecond(
                                 _patternStart + _noteList.Last().Position + _noteList.Last().Duration + 120, _aceTempoList) 
                             - patternStartSecond;

            var segments = curve.SplitIntoSegments(-100)
                .Where(seg =>
                {
                    if (seg.Last().Item1 < _firstBarTicks)
                    {
                        return false;
                    }
                    var startSec = seg.First().Item1 >= _firstBarTicks
                        ? _synchronizer.GetActualSecsFromTicks(seg.First().Item1 - _firstBarTicks) - patternStartSecond
                        : 0;
                    var endSec = _synchronizer.GetActualSecsFromTicks(seg.Last().Item1 - _firstBarTicks) - patternStartSecond;
                    return startSec <= rightBound && endSec >= leftBound;
                });

            foreach (var segment in segments)
            {
                var startPoint = segment.BinaryFindLast(point =>
                                     point.Item1 >= _firstBarTicks
                                     && _synchronizer.GetActualSecsFromTicks(point.Item1 - _firstBarTicks) <= patternStartSecond + leftBound)
                                 ?? segment.First();

                var endPoint = segment.BinaryFindFirst(point =>
                                   point.Item1 >= _firstBarTicks
                                   && _synchronizer.GetActualSecsFromTicks(point.Item1 - _firstBarTicks) >= patternStartSecond + rightBound)
                               ?? segment.Last();
                
                var aceCurve = new AceParamCurve
                {
                    Offset = (int) Math.Round(_synchronizer.GetActualTicksFromTicks(startPoint.Item1 - _firstBarTicks)) - _patternStart
                };
                var curveEnd = (int) Math.Round(_synchronizer.GetActualTicksFromTicks(endPoint.Item1 - _firstBarTicks)) - _patternStart;
                
                var tickStep = (double) (endPoint.Item1 - startPoint.Item1) / (curveEnd - aceCurve.Offset);
                double tick = startPoint.Item1;
                for (; tick < _firstBarTicks; ++aceCurve.Offset, tick += tickStep) { }
                tick = Math.Max(_firstBarTicks, Math.Round(tick));
                tickStep = (endPoint.Item1 - tick) / (curveEnd - aceCurve.Offset);
                
                var secondStep = _synchronizer.GetDurationSecsFromTicks(
                                     (int) tick - _firstBarTicks,
                                     endPoint.Item1 - _firstBarTicks)
                                 / (curveEnd - aceCurve.Offset);
                var second = _synchronizer.GetActualSecsFromTicks((int) tick - _firstBarTicks) - patternStartSecond;
                
                for (; second < leftBound; ++aceCurve.Offset, tick += tickStep, second += secondStep) { }
                
                for (var pos = aceCurve.Offset;
                     pos <= curveEnd && second <= rightBound;
                     ++pos, tick += tickStep, second += secondStep)
                {
                    aceCurve.Values.Add(
                        CurveSegmentUtils.GetValueFromSegment(segment, tick) / 100 - basePitch.SemitoneValueAt(second));
                }
                
                aceCurves.Add(aceCurve);
            }
            return aceCurves;
        }

        private List<AceParamCurve> EncodeParamCurves(ParamCurve curve, Func<double, double> mappingFunc)
        {
            var aceCurves = new List<AceParamCurve>();
            var patternStartSecond = TimeUtils.TickToSecond(_patternStart, _aceTempoList);
            var leftBound = TimeUtils.TickToSecond(
                                Math.Max(0, _patternStart + _noteList.First().Position - 240), _aceTempoList)
                            - patternStartSecond;
            var rightBound = TimeUtils.TickToSecond(
                                 _patternStart + _noteList.Last().Position + _noteList.Last().Duration + 120, _aceTempoList) 
                             - patternStartSecond;
            
            var segments = curve.SplitIntoSegments(-100)
                .Where(seg =>
                {
                    if (seg.Last().Item1 < _firstBarTicks)
                    {
                        return false;
                    }
                    var startSec = seg.First().Item1 >= _firstBarTicks
                        ? _synchronizer.GetActualSecsFromTicks(seg.First().Item1 - _firstBarTicks) - patternStartSecond
                        : 0;
                    var endSec = _synchronizer.GetActualSecsFromTicks(seg.Last().Item1 - _firstBarTicks) - patternStartSecond;
                    return startSec <= rightBound && endSec >= leftBound;
                });

            foreach (var segment in segments)
            {
                var startPoint = segment.BinaryFindLast(point =>
                                     point.Item1 >= _firstBarTicks
                                     && _synchronizer.GetActualSecsFromTicks(point.Item1 - _firstBarTicks) <= patternStartSecond + leftBound)
                                 ?? segment.First();

                var endPoint = segment.BinaryFindFirst(point =>
                                   point.Item1 >= _firstBarTicks
                                   && _synchronizer.GetActualSecsFromTicks(point.Item1 - _firstBarTicks) >= patternStartSecond + rightBound)
                               ?? segment.Last();
                
                var aceCurve = new AceParamCurve
                {
                    Offset = (int) Math.Round(_synchronizer.GetActualTicksFromTicks(startPoint.Item1 - _firstBarTicks)) - _patternStart
                };
                var curveEnd = (int)Math.Round(_synchronizer.GetActualTicksFromTicks(endPoint.Item1 - _firstBarTicks)) - _patternStart;
                
                var tickStep = (double) (endPoint.Item1 - startPoint.Item1) / (curveEnd - aceCurve.Offset);
                double tick = startPoint.Item1;
                for (; tick < _firstBarTicks; ++aceCurve.Offset, tick += tickStep) { }
                tick = Math.Max(_firstBarTicks, Math.Round(tick));
                tickStep = (endPoint.Item1 - tick) / (curveEnd - aceCurve.Offset);
                
                var secondStep = _synchronizer.GetDurationSecsFromTicks(
                                     (int) tick - _firstBarTicks,
                                     endPoint.Item1 - _firstBarTicks)
                                 / (curveEnd - aceCurve.Offset);
                var second = _synchronizer.GetActualSecsFromTicks((int) tick - _firstBarTicks) - patternStartSecond;
                
                for (; second < leftBound; ++aceCurve.Offset, tick += tickStep, second += secondStep) { }
                
                for (var pos = aceCurve.Offset;
                     pos <= curveEnd && second <= rightBound;
                     ++pos, tick += tickStep)
                {
                    aceCurve.Values.Add(mappingFunc(CurveSegmentUtils.GetValueFromSegment(segment, tick)));
                }
                
                aceCurves.Add(aceCurve);
            }
            return aceCurves;
        }
        
        private int EncodeAudioOffset(int offset)
        {
            if (!_hasMultiTempo)
            {
                return offset;
            }
            if (offset > 0)
            {
                return (int) Math.Round(_synchronizer.GetActualTicksFromTicks(offset));
            }
            var currentPos = _firstBarTicks;
            var actualPos = _firstBarTicks + offset;
            var res = 0.0;
            var i = _firstBarTempo.Count - 1;
            for (; i >= 0 && actualPos <= _firstBarTempo[i].Position; i--)
            {
                res -= (currentPos - _firstBarTempo[i].Position) * DefaultTempo / _firstBarTempo[i].BPM;
                currentPos = _firstBarTempo[i].Position;
            }
            if (i >= 0)
            {
                res -= (currentPos - actualPos) * DefaultTempo / _firstBarTempo[i].BPM;
            }
            else
            {
                res += actualPos * DefaultTempo / _firstBarTempo[0].BPM;
            }
            return (int) Math.Round(res);
        }
    }
}