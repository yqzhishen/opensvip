using System;
using System.Collections.Generic;
using System.Linq;
using OpenSvip.Library;
using SynthV.Model;

namespace Plugin.SynthV
{
    public class PitchGenerator
    {
        private readonly TimeSynchronizer Synchronizer;

        private readonly CurveGenerator PitchDiff;

        private readonly CurveGenerator VibratoEnv;

        private readonly List<PitchNode> PitchNodes = new List<PitchNode>();

        private readonly List<VibratoNode> VibratoNodes = new List<VibratoNode>();

        private const double MinimumInterval = 0.01;

        private readonly Dictionary<string, Func<double, double>> InterpolationDict =
            new Dictionary<string, Func<double, double>>
            {
                {"linear", Interpolation.LinearInterpolation()},
                {"cosine", Interpolation.CosineInterpolation()},
                {"cubic", Interpolation.CubicInterpolation()}
            };

        public PitchGenerator(
            TimeSynchronizer synchronizer,
            List<SVNote> noteList,
            SVParamCurve pitchDiff,
            SVParamCurve vibratoEnv)
        {
            Synchronizer = synchronizer;
            PitchDiff = new CurveGenerator(
                pitchDiff.Points.ConvertAll(point => new Tuple<int, int>(
                    PosToTicks(point.Item1), (int) Math.Round(point.Item2))),
                InterpolationDict[pitchDiff.Mode],
                0);
            VibratoEnv = new CurveGenerator(
                vibratoEnv.Points.ConvertAll(point => new Tuple<int, int>(
                    PosToTicks(point.Item1), (int) Math.Round(point.Item2 * 1000.0))),
                InterpolationDict[vibratoEnv.Mode],
                1000);
            
            if (!noteList.Any())
            {
                return;
            }

            for (var i = 0; i < noteList.Count; i++)
            {
                var start = Synchronizer.GetActualSecsFromTicks(PosToTicks(noteList[i].Onset));
                var end = Synchronizer.GetActualSecsFromTicks(PosToTicks(noteList[i].Onset + noteList[i].Duration));

                if (!(end - start > noteList[i].Attributes.VibratoStart))
                {
                    continue;
                }
                if (i < noteList.Count - 1
                    && Synchronizer.GetActualSecsFromTicks(PosToTicks(noteList[i + 1].Onset)) - end < MinimumInterval)
                {
                    end += Math.Min(
                        noteList[i + 1].Attributes.TransitionOffset,
                        Math.Min(
                            noteList[i + 1].Attributes.VibratoStart,
                            Synchronizer.GetDurationSecsFromTicks(
                                PosToTicks(noteList[i + 1].Onset),
                                PosToTicks(noteList[i + 1].Onset + noteList[i + 1].Duration))));
                }
                if (start < end)
                {
                    VibratoNodes.Add(new VibratoNode
                    {
                        Begin = start + noteList[i].Attributes.VibratoStart,
                        End = end,
                        FadeLeft = noteList[i].Attributes.VibratoLeft,
                        FadeRight = noteList[i].Attributes.VibratoRight,
                        Amplitude = noteList[i].Attributes.VibratoDepth / 2,
                        Frequency = noteList[i].Attributes.VibratoFrequency,
                        Phase = noteList[i].Attributes.VibratoPhase
                    });
                }
            }
            var currentNote = noteList[0];
            var currentBegin = Synchronizer.GetActualSecsFromTicks(PosToTicks(currentNote.Onset));
            var currentEnd = Synchronizer.GetActualSecsFromTicks(PosToTicks(currentNote.Onset + currentNote.Duration));
            var currentMain = Math.Min(currentBegin + currentNote.Attributes.SlideLeft, currentEnd);
            PitchNodes.Add(new PlainNode
            {
                Begin = double.MinValue,
                End = currentBegin,
                Pitch = (currentNote.Pitch - currentNote.Attributes.DepthLeft) * 100
            });
            // head part of the first note
            PitchNodes.Add(new BoundaryNode
            {
                Begin = currentBegin,
                End = Math.Min(currentEnd, currentBegin + currentNote.Attributes.SlideLeft),
                IsHead = true,
                BaseKey = currentNote.Pitch,
                Depth = currentNote.Attributes.DepthLeft
            });
            var j = 0;
            for (; j < noteList.Count - 1; j++)
            {
                var nextNote = noteList[j + 1];
                var nextBegin = Synchronizer.GetActualSecsFromTicks(PosToTicks(nextNote.Onset));
                var nextEnd = Synchronizer.GetActualSecsFromTicks(PosToTicks(nextNote.Onset + nextNote.Duration));
                double nextMain;
                double nextBody;
                
                var interval = nextBegin - currentEnd;
                if (interval > MinimumInterval) // two notes are far from each other
                {
                    nextMain = nextBegin;
                    
                    // main part of current note
                    PitchNodes.Add(new PlainNode
                    {
                        Begin = currentMain,
                        End = currentEnd,
                        Pitch = currentNote.Pitch * 100
                    });
                    
                    nextBody = Math.Min(nextBegin + nextNote.Attributes.SlideLeft, nextEnd);
                    var mainEnd = Math.Max(currentBegin, currentEnd - currentNote.Attributes.SlideRight);

                    // tail part of current note
                    PitchNodes.Add(new BoundaryNode
                    {
                        Begin = mainEnd,
                        End = currentEnd,
                        IsHead = false,
                        BaseKey = currentNote.Pitch,
                        Depth = currentNote.Attributes.DepthRight
                    });
                    
                    // interpolation between two notes
                    var mid = (currentEnd + nextBegin) / 2;
                    if (nextBegin - currentEnd > 0.14)
                    {
                        PitchNodes.Add(new PlainNode
                        {
                            Begin = currentEnd,
                            End = mid - 0.07,
                            Pitch = (currentNote.Pitch - currentNote.Attributes.DepthRight) * 100
                        });
                        PitchNodes.Add(new TransitionNode
                        {
                            Begin = mid - 0.07,
                            End = mid + 0.07,
                            Middle = mid,
                            PitchLeft = (currentNote.Pitch - currentNote.Attributes.DepthRight) * 100,
                            PitchRight = (nextNote.Pitch - nextNote.Attributes.DepthLeft) * 100,
                            GaussBegin = mid - 0.07,
                            GaussEnd = mid + 0.07,
                            DepthLeft = 0,
                            DepthRight = 0
                        });
                        PitchNodes.Add(new PlainNode
                        {
                            Begin = mid + 0.07,
                            End = nextBegin,
                            Pitch = (nextNote.Pitch - nextNote.Attributes.DepthLeft) * 100
                        });
                    }
                    else
                    {
                        PitchNodes.Add(new TransitionNode
                        {
                            Begin = currentEnd,
                            End = nextBegin,
                            Middle = mid,
                            PitchLeft = (currentNote.Pitch - currentNote.Attributes.DepthRight) * 100,
                            PitchRight = (nextNote.Pitch - nextNote.Attributes.DepthLeft) * 100,
                            GaussBegin = currentEnd,
                            GaussEnd = nextBegin,
                            DepthLeft = 0,
                            DepthRight = 0
                        });
                    }
                    
                    // head part of next note
                    PitchNodes.Add(new BoundaryNode
                    {
                        Begin = nextBegin,
                        End = nextBody,
                        IsHead = true,
                        BaseKey = nextNote.Pitch,
                        Depth = nextNote.Attributes.DepthLeft
                    });
                }
                else // two notes are close to each other
                {
                    nextMain = Math.Max((currentBegin + currentEnd) / 2, Math.Min((nextBegin + nextEnd) / 2,
                        nextBegin + nextNote.Attributes.TransitionOffset));
                    
                    // main part of current note
                    if (currentMain < nextMain)
                    {
                        PitchNodes.Add(new PlainNode
                        {
                            Begin = currentMain,
                            End = nextMain,
                            Pitch = currentNote.Pitch * 100
                        });
                    }
                    
                    nextBody = Math.Max((currentBegin + currentEnd) / 2, Math.Min((nextBegin + nextEnd) / 2,
                        nextBegin + 1.25 * nextNote.Attributes.SlideLeft + nextNote.Attributes.TransitionOffset));
                    var mainEnd = Math.Max((currentBegin + currentEnd) / 2, Math.Min((nextBegin + nextEnd) / 2,
                        currentEnd - currentNote.Attributes.SlideRight + nextNote.Attributes.TransitionOffset + interval));
                    
                    // transition part of two notes
                    double Sigmoid(double x)
                    {
                        return 2 / (1 + Math.Exp(-2.2 * x)) - 1;
                    }

                    double Average(double x, double y)
                    {
                        return Math.Sqrt(x * y);
                    }
                    
                    var node = new TransitionNode
                    {
                        Middle = Math.Max(mainEnd, Math.Min(nextBody,
                            (currentEnd + nextBegin) / 2 + nextNote.Attributes.TransitionOffset)),
                        PitchLeft = currentNote.Pitch * 100,
                        PitchRight = nextNote.Pitch * 100
                    };
                    var slideLength = currentNote.Attributes.SlideRight + nextNote.Attributes.SlideLeft;
                    node.Begin = Math.Max(currentBegin, node.Middle - slideLength);
                    node.End = Math.Min(nextEnd, node.Middle + slideLength);
                    var ratio = Sigmoid(0.125 * Average(
                        (node.Middle - currentBegin) / currentNote.Attributes.SlideRight,
                        (nextEnd - node.Middle) / nextNote.Attributes.SlideLeft));
                    node.GaussBegin = mainEnd * ratio + node.Middle * (1 - ratio);
                    node.GaussEnd = nextBody * ratio + node.Middle * (1 - ratio);
                    node.DepthLeft = currentNote.Attributes.DepthRight * Math.Pow(ratio, 0.75);
                    node.DepthRight = nextNote.Attributes.DepthLeft * Math.Pow(ratio, 0.75);
                    node.Zoom = Math.Min(10, 1 / Math.Pow(ratio, 2.2));
                    PitchNodes.Add(node);
                }
                
                currentNote = nextNote;
                currentBegin = nextBegin;
                currentEnd = nextEnd;
                currentMain = nextMain;
            }
            // main part of the last note
            PitchNodes.Add(new PlainNode
            {
                Begin = currentMain,
                End = currentEnd,
                Pitch = currentNote.Pitch * 100
            });
            
            // tail part of the last note
            PitchNodes.Add(new BoundaryNode
            {
                Begin = Math.Max(currentBegin, currentEnd - currentNote.Attributes.SlideRight),
                End = currentEnd,
                IsHead = false,
                BaseKey = currentNote.Pitch,
                Depth = currentNote.Attributes.DepthRight
            });
            PitchNodes.Add(new PlainNode
            {
                Begin = currentEnd,
                End = double.MaxValue,
                Pitch = (currentNote.Pitch - currentNote.Attributes.DepthRight) * 100
            });
        }

        public int PitchAtTicks(int ticks)
        {
            return PitchAtSecs(Synchronizer.GetActualSecsFromTicks(ticks));
        }

        public int PitchAtSecs(double secs)
        {
            var nodeHits = PitchNodes.FindAll(node => node.Begin <= secs && node.End > secs);
            var basePitch = nodeHits.Average(node => node.BasePitch(secs)) +
                            nodeHits.Sum(node => node.PitchAtSecs(secs) - node.BasePitch(secs));
            var vibrato = VibratoNodes.Find(node => node.Begin <= secs && node.End > secs);
            var ticks = (int) Math.Round(Synchronizer.GetActualTicksFromSecs(secs));
            if (vibrato == null)
            {
                return (int) Math.Round(basePitch + PitchDiff.ValueAtTicks(ticks));
            }
            return (int) Math.Round(basePitch
                   + vibrato.PitchDiffAtSecs(secs) * VibratoEnv.ValueAtTicks(ticks) / 1000.0
                   + PitchDiff.ValueAtTicks(ticks));
        }

        private static int PosToTicks(long pos)
        {
            return (int) Math.Round(pos / 1470000.0);
        }

        private static Func<double, double> GaussFunc(double a, double b, double c)
        {
            return x => a * Math.Exp(-Math.Pow((x - b) / c, 2) / 2);
        }
        
        private abstract class PitchNode
        {
            public double Begin;
            public double End;

            public abstract double PitchAtSecs(double secs);

            public abstract double BasePitch(double secs);
        }

        private class PlainNode : PitchNode
        {
            public double Pitch;

            public override double PitchAtSecs(double secs)
            {
                return Pitch;
            }

            public override double BasePitch(double secs)
            {
                return Pitch;
            }
        }

        private class BoundaryNode : PitchNode
        {
            public bool IsHead;
            public int BaseKey;
            public double Depth;

            public override double PitchAtSecs(double secs)
            {
                var gauss = GaussFunc(Depth, IsHead ? Begin : End, 0.3 * (End - Begin));
                return (BaseKey - gauss(secs)) * 100;
            }

            public override double BasePitch(double secs)
            {
                return BaseKey * 100;
            }
        }

        private class TransitionNode : PitchNode
        {
            public double PitchLeft;
            public double PitchRight;
            public double Middle;
            public double GaussBegin;
            public double GaussEnd;
            public double DepthLeft;
            public double DepthRight;
            public double Zoom = 1.0;

            public override double PitchAtSecs(double secs)
            {
                var gaussLeft = Middle - GaussBegin >= MinimumInterval / 2
                    ? GaussFunc(DepthLeft * 100, GaussBegin * 0.425 + Middle * 0.575, 0.3 * Zoom * (Middle - GaussBegin))
                    : x => 0;
                var gaussRight = GaussEnd - Middle >= MinimumInterval / 2
                    ? GaussFunc(DepthRight * 100, GaussEnd * 0.425 + Middle * 0.575, 0.3 * Zoom * (GaussEnd - Middle))
                    : x => 0;

                double Sigmoid(double x)
                {
                    var r = Interpolation.SigmoidInterpolation(11.0)((x - Begin) / (End - Begin));
                    return (1 - r) * PitchLeft + r * PitchRight;
                }

                return PitchLeft >= PitchRight
                    ? Sigmoid(secs) + gaussLeft(secs) - gaussRight(secs)
                    : Sigmoid(secs) - gaussLeft(secs) + gaussRight(secs);
            }

            public override double BasePitch(double secs)
            {
                return secs < Middle ? PitchLeft : PitchRight;
            }
        }

        private class VibratoNode
        {
            public double Begin;
            public double End;
            public double FadeLeft;
            public double FadeRight;
            public double Amplitude;
            public double Frequency;
            public double Phase;

            public double PitchDiffAtSecs(double secs)
            {
                double zoom;
                if (End - Begin >= FadeLeft + FadeLeft)
                {
                    if (secs < Begin + FadeLeft)
                    {
                        zoom = (secs - Begin) / FadeLeft;
                    }
                    else if (secs > End - FadeRight)
                    {
                        zoom = (End - secs) / FadeRight;
                    }
                    else
                    {
                        zoom = 1.0;
                    }
                }
                else
                {
                    var mid = (Begin * FadeRight + End * FadeLeft) / (FadeLeft + FadeRight);
                    zoom = secs < mid
                        ? (secs - Begin) / FadeLeft
                        : (End - secs) / FadeRight;
                }
                return zoom * Amplitude * 100.0 * Math.Sin(2 * Math.PI * Frequency * (secs - Begin) + Phase);
            }
        }
    }

}
