using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using OpenSvip.Model;
using SynthV.Model;

namespace Plugin.SynthV
{
    public class SynthVEncoder
    {
        public SynthVEncoder() { }

        public SynthVEncoder(int defaultTempo)
        {
            DefaultTempo = defaultTempo;
        }
        
        private bool IsAbsoluteTimeMode;

        private readonly int DefaultTempo = 60;

        private int FirstBarTick;

        private List<SongTempo> FirstBarTempo;

        private List<SongTempo> TempoBuffer;

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
            if (newMeters.Any(meter => meter.Denominator < 2 || meter.Denominator > 16))
            {
                IsAbsoluteTimeMode = true;
                TempoBuffer = newTempos;
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
            foreach (var track in project.TrackList)
            {
                var svTrack = EncodeTrack(track);
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
                    svTrack.MainGroup.Params = EncodeParams(singingTrack.EditedParams);
                    foreach (var note in singingTrack.NoteList)
                    {
                        svTrack.MainGroup.Notes.Add(EncodeNote(note));
                    }
                    // TODO: other singing track attributes
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
                // TODO: pitch
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

        private SVParamCurve EncodeParamCurve(ParamCurve curve, int termination, double defaultValue, Func<int, double> op)
        {
            var svCurve = new SVParamCurve();
            var pointList = svCurve.Points;
            var buffer = new List<Tuple<int, int>>();
            const int minInterval = 16;
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

        private SVNote EncodeNote(Note note)
        {
            var svNote = new SVNote
            {
                Onset = EncodePosition(note.StartPos)
            };
            svNote.Duration = EncodePosition(note.StartPos + note.Length) - svNote.Onset;
            svNote.Pitch = note.KeyNumber;
            svNote.Lyrics = note.Pronunciation ?? note.Lyric;
            // TODO: other attributes
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
                // TODO: is this right??
            }
            double res = 0.0;
            var i = 0;
            for (; i < TempoBuffer.Count - 1 && TempoBuffer[i + 1].Position < position; i++)
            {
                res += (TempoBuffer[i + 1].Position - TempoBuffer[i].Position) * DefaultTempo / TempoBuffer[i].BPM;
            }
            res += (position - TempoBuffer[i].Position) * DefaultTempo / TempoBuffer[i].BPM;
            return (long) Math.Round(res * 1470000L);
        }
    }
}
