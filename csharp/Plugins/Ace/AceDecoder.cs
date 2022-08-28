using System;
using System.Collections.Generic;
using System.Linq;
using AceStdio.Model;
using AceStdio.Options;
using AceStdio.Param;
using AceStdio.Utils;
using OpenSvip.Library;
using OpenSvip.Model;

namespace AceStdio.Core
{
    public class AceDecoder
    {
        public bool KeepAllPronunciation { get; set; }
        
        public bool ImportTension { get; set; }
        
        public bool ImportEnergy { get; set; }
        
        public double EnergyCoefficient { get; set; }

        public int SampleInterval { get; set; }
        
        public NormalizationArgs BreathNormArgs { get; set; }
        
        public NormalizationArgs TensionNormArgs { get; set; }
        
        public NormalizationArgs EnergyNormArgs { get; set; }
        
        private int _contentVersion;

        private List<AceTempo> _aceTempoList;

        private TimeSynchronizer _synchronizer;

        private int _firstBarTicks;

        private List<AceNote> _aceNoteList;

        public Project DecodeProject(AceContent content)
        {
            var project = new Project();
            _contentVersion = content.Version;
            _aceTempoList = content.TempoList;
            project.TimeSignatureList.Add(new TimeSignature
            {
                BarIndex = 0,
                Numerator = content.BeatsPerBar,
                Denominator = 4
            });
            _firstBarTicks = 480 * content.BeatsPerBar;
            project.SongTempoList = ScoreMarkUtils.ShiftTempoList(content.TempoList.ConvertAll(DecodeTempo), _firstBarTicks);
            _synchronizer = new TimeSynchronizer(project.SongTempoList);

            foreach (var track in content.TrackList)
            {
                track.Gain += content.Master.Gain;
            }
            project.TrackList.AddRange(content.TrackList.Select(DecodeTrack).Where(track => track != null));
            return project;
        }

        private SongTempo DecodeTempo(AceTempo tempo)
        {
            return new SongTempo
            {
                Position = tempo.Position,
                BPM = (float) tempo.BPM
            };
        }

        private Track DecodeTrack(AceTrack aceTrack)
        {
            Track track;
            switch (aceTrack)
            {
                case AceVocalTrack aceVocalTrack:
                    var singingTrack = new SingingTrack
                    {
                        AISingerName = aceVocalTrack.Singer
                    };

                    _aceNoteList = new List<AceNote>();
                    var aceParams = new AceParams();
                    foreach (var pattern in aceVocalTrack.PatternList.Where(pattern => pattern.NoteList.Any()))
                    {
                        var noteList = pattern.NoteList
                            .Where(note =>
                                note.Position + pattern.Position >= 0
                                && note.Position >= pattern.ClipPosition
                                && note.Position < pattern.ClipPosition + pattern.ClipDuration);

                        foreach (var aceNote in noteList)
                        {
                            aceNote.Duration = Math.Min(aceNote.Duration,
                                pattern.ClipPosition + pattern.ClipDuration - aceNote.Position);
                            aceNote.Position += pattern.Position;
                            _aceNoteList.Add(aceNote);
                        }

                        void MergeCurves(IEnumerable<AceParamCurve> src, ICollection<AceParamCurve> dst)
                        {
                            var curveList = src
                                .Where(curve =>
                                    curve.Offset + pattern.Position >= -_firstBarTicks
                                    && curve.Offset + curve.Values.Count > pattern.ClipPosition
                                    && curve.Offset < pattern.ClipPosition + pattern.ClipDuration);

                            foreach (var aceCurve in curveList)
                            {
                                var maxLength = pattern.ClipPosition + pattern.ClipDuration - aceCurve.Offset;
                                if (maxLength < aceCurve.Values.Count)
                                {
                                    aceCurve.Values.RemoveRange(maxLength, aceCurve.Values.Count - maxLength);
                                }
                                aceCurve.Offset += pattern.Position;
                                dst.Add(aceCurve);
                            }
                        }
                        
                        MergeCurves(pattern.Params.PitchDelta, aceParams.PitchDelta);
                        MergeCurves(pattern.Params.Breath, aceParams.Breath);
                        MergeCurves(pattern.Params.Gender, aceParams.Gender);
                        MergeCurves(pattern.Params.Energy, aceParams.Energy);
                        MergeCurves(pattern.Params.Tension, aceParams.Tension);
                        if (BreathNormArgs.IsEnabled)
                        {
                            MergeCurves(pattern.Params.RealBreath, aceParams.RealBreath);
                        }
                        if (TensionNormArgs.IsEnabled)
                        {
                            MergeCurves(pattern.Params.RealTension, aceParams.RealTension);
                        }
                        if (EnergyNormArgs.IsEnabled)
                        {
                            MergeCurves(pattern.Params.RealEnergy, aceParams.RealEnergy);
                        }
                    }

                    if (KeepAllPronunciation)
                    {
                        singingTrack.NoteList = _aceNoteList.ConvertAll(note => DecodeNote(note));
                    }
                    else
                    {
                        var pinyinSeries = PinyinUtils.GetPinyinSeries(_aceNoteList.Select(note => note.Lyrics));
                        singingTrack.NoteList = _aceNoteList.Zip(pinyinSeries, DecodeNote).ToList();
                    }
                    singingTrack.EditedParams = DecodeParams(aceParams);
                    track = singingTrack;
                    break;
                case AceAudioTrack aceAudioTrack:
                    if (!aceAudioTrack.PatternList.Any())
                    {
                        return null;
                    }
                    var instrumentalTrack = new InstrumentalTrack
                    {
                        AudioFilePath = aceAudioTrack.PatternList[0].AudioFilePath
                    };
                    track = instrumentalTrack;
                    break;
                default:
                    throw new ArgumentException(nameof(aceTrack));
            }
            track.Title = aceTrack.Name;
            track.Mute = aceTrack.Mute;
            track.Solo = aceTrack.Solo;
            track.Volume = Math.Pow(10, aceTrack.Gain / 20);
            return track;
        }

        private Note DecodeNote(AceNote aceNote, string pinyin = null)
        {
            var note = new Note
            {
                KeyNumber = aceNote.Pitch,
                StartPos = aceNote.Position,
                Length = aceNote.Duration,
                Lyric = aceNote.Lyrics
            };
            if (pinyin == null || !aceNote.Lyrics.Contains('-') && aceNote.Pronunciation != pinyin)
            {
                note.Pronunciation = aceNote.Pronunciation;
            }
            if (aceNote.BreathLength > 0)
            {
                note.HeadTag = "V";
            }
            if (aceNote.ConsonantLength != null)
            {
                // TODO: this may cause negative time coordinates, producing exception in TimeSynchronizer
                note.EditedPhones = new Phones
                {
                    HeadLengthInSecs = (float)
                        (_synchronizer.GetActualSecsFromTicks(note.StartPos)
                         - _synchronizer.GetActualSecsFromTicks(note.StartPos - aceNote.ConsonantLength.Value))
                };
            }
            return note;
        }

        private Params DecodeParams(AceParams aceParams)
        {
            Func<double, int> LinearTransform(double lowerBound, double middleValue, double upperBound)
            {
                return x => Math.Max(-1000, Math.Min(1000, (int) Math.Round(
                    x >= middleValue
                        ? (x - middleValue) / (upperBound - middleValue) * 1000
                        : (x - middleValue) / (middleValue - lowerBound) * 1000)));
            }
            var oldVersionTransform = LinearTransform(0, 0.5, 1.0);

            // Normalize real breath
            if (BreathNormArgs.IsEnabled)
            {
                var normalized = aceParams.RealBreath
                    .Exclude(x => x + 1e-3 < BreathNormArgs.LowerThreshold || x - 1e-3 > BreathNormArgs.UpperThreshold);
                switch (BreathNormArgs.Method)
                {
                    case NormalizationMethod.ZScore:
                        normalized = normalized.ZScoreNormalize(BreathNormArgs.Scale, BreathNormArgs.Bias);
                        break;
                    case NormalizationMethod.MinMax:
                        normalized = normalized.MinMaxNormalize(BreathNormArgs.Scale, BreathNormArgs.Bias);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(BreathNormArgs.Method));
                }
                
                if (_contentVersion >= 1)
                {
                    aceParams.Breath = aceParams.Breath.Plus(
                        normalized,
                        1.0,
                        x => x >= 0 ? x * 1.5 : x * 0.8);
                }
                else
                {
                    aceParams.Breath = aceParams.Breath.Plus(
                        normalized,
                        0.5,
                        x => x / 2);
                }
            }
            
            // Normalize real tension
            if (TensionNormArgs.IsEnabled)
            {
                var normalized = aceParams.RealTension
                    .Exclude(x => x + 1e-3 < TensionNormArgs.LowerThreshold || x - 1e-3 > TensionNormArgs.UpperThreshold);
                switch (TensionNormArgs.Method)
                {
                    case NormalizationMethod.ZScore:
                        normalized = normalized.ZScoreNormalize(TensionNormArgs.Scale, TensionNormArgs.Bias);
                        break;
                    case NormalizationMethod.MinMax:
                        normalized = normalized.MinMaxNormalize(TensionNormArgs.Scale, TensionNormArgs.Bias);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(TensionNormArgs.Method));
                }
                
                if (_contentVersion >= 1)
                {
                    aceParams.Tension = aceParams.Tension.Plus(
                        normalized,
                        1.0,
                        x => x >= 0 ? x * 0.5 : x * 0.3);
                }
                else
                {
                    aceParams.Tension = aceParams.Tension.Plus(
                        normalized,
                        0.5,
                        x => x / 2);
                }
            }
            
            // Normalize real energy
            if (EnergyNormArgs.IsEnabled)
            {
                var normalized = aceParams.RealEnergy
                    .Exclude(x => x + 1e-3 < EnergyNormArgs.LowerThreshold || x - 1e-3 > EnergyNormArgs.UpperThreshold);
                switch (EnergyNormArgs.Method)
                {
                    case NormalizationMethod.ZScore:
                        normalized = normalized.ZScoreNormalize(EnergyNormArgs.Scale, EnergyNormArgs.Bias);
                        break;
                    case NormalizationMethod.MinMax:
                        normalized = normalized.MinMaxNormalize(EnergyNormArgs.Scale, EnergyNormArgs.Bias);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(EnergyNormArgs.Method));
                }
                
                if (_contentVersion >= 1)
                {
                    aceParams.Energy = aceParams.Energy.Plus(
                        normalized,
                        1.0,
                        x => x);
                }
                else
                {
                    aceParams.Energy = aceParams.Energy.Plus(
                        normalized,
                        0.5,
                        x => x / 2);
                }
            }

            var parameters = new Params
            {
                Pitch = DecodePitchCurve(aceParams.PitchDelta),
                Breath = DecodeParamCurve(aceParams.Breath,
                    _contentVersion >= 1 ? LinearTransform(0.2, 1, 2.5) : oldVersionTransform),
                Gender = DecodeParamCurve(aceParams.Gender,
                    _contentVersion >= 1 ? LinearTransform(-1, 0, 1) : oldVersionTransform)
            };
            if (ImportTension && ImportEnergy)
            {
                parameters.Volume = DecodeParamCurve(
                    aceParams.Energy,
                    x =>
                    {
                        var value = _contentVersion >= 1
                            ? LinearTransform(0, 1, 2)(x)
                            : oldVersionTransform(x);
                        return (int) Math.Round(EnergyCoefficient * value);
                    });
                var remainingEnergy = aceParams.Energy.ConvertAll(curve => new AceParamCurve
                {
                    Offset = curve.Offset,
                    Values = curve.Values.ConvertAll(x => _contentVersion >= 1
                        ? (x - 1.0) * (1 - EnergyCoefficient) + 1.0
                        : (x - 0.5) * (1 - EnergyCoefficient) + 0.5)
                });
                var energyPlusTension = _contentVersion >= 1
                    ? remainingEnergy.Plus(
                        aceParams.Tension,
                        1.0,
                        x => x >= 1.0 ? (x - 1.0) / 0.5 : (x - 1.0) / 0.3)
                    : remainingEnergy.Plus(
                        aceParams.Tension,
                        0.5,
                        x => x - 0.5);
                parameters.Strength = DecodeParamCurve(energyPlusTension,
                    _contentVersion >= 1 ? LinearTransform(0, 1, 2) : oldVersionTransform);
            }
            else if (ImportTension)
            {
                parameters.Strength = DecodeParamCurve(aceParams.Tension,
                    _contentVersion >= 1 ? LinearTransform(0.7, 1, 1.5) : oldVersionTransform);
            }
            else if (ImportEnergy)
            {
                var transform = _contentVersion >= 1 ? LinearTransform(0, 1, 2) : oldVersionTransform;
                parameters.Volume = DecodeParamCurve(aceParams.Energy,
                    x => (int) Math.Round(EnergyCoefficient * transform(x)));
                parameters.Strength = DecodeParamCurve(aceParams.Energy,
                    x => (int) Math.Round((1 - EnergyCoefficient) * transform(x)));
            }
            return parameters;
        }

        private ParamCurve DecodePitchCurve(List<AceParamCurve> aceCurveList)
        {
            var curve = new ParamCurve();
            curve.PointList.Add(new Tuple<int, int>(-192000, -100));
            if (!_aceNoteList.Any())
            {
                goto done;
            }
            var basePitch = new BasePitchCurve(_aceNoteList, _aceTempoList);
            foreach (var aceCurve in aceCurveList)
            {
                var pos = aceCurve.Offset;
                curve.PointList.Add(new Tuple<int, int>(pos + _firstBarTicks, -100));
                foreach (var absSemitone in aceCurve.Values.Select(value =>
                             basePitch.SemitoneValueAt(_synchronizer.GetActualSecsFromTicks(pos)) + value))
                {
                    curve.PointList.Add(new Tuple<int, int>(pos + _firstBarTicks, (int) Math.Round(absSemitone * 100)));
                    ++pos;
                }
                curve.PointList.Add(new Tuple<int, int>(pos - 1 + _firstBarTicks, -100));
            }
            done:
            curve.PointList.Add(new Tuple<int, int>(int.MaxValue / 2, -100));
            if (SampleInterval > 0)
            {
                curve = curve.ReduceSampleRate(SampleInterval, -100);
            }
            return curve;
        }

        private ParamCurve DecodeParamCurve(List<AceParamCurve> aceCurveList, Func<double, int> mappingFunc)
        {
            var curve = new ParamCurve();
            curve.PointList.Add(new Tuple<int, int>(-192000, 0));
            foreach (var aceCurve in aceCurveList)
            {
                var pos = aceCurve.Offset;
                curve.PointList.Add(new Tuple<int, int>(pos + _firstBarTicks, 0));
                foreach (var value in aceCurve.Values)
                {
                    curve.PointList.Add(new Tuple<int, int>(pos + _firstBarTicks, mappingFunc(value)));
                    ++pos;
                }
                curve.PointList.Add(new Tuple<int, int>(pos - 1 + _firstBarTicks, 0));
            }
            curve.PointList.Add(new Tuple<int, int>(int.MaxValue / 2, 0));
            if (SampleInterval > 0)
            {
                curve = curve.ReduceSampleRate(SampleInterval);
            }
            return curve;
        }
    }
}
