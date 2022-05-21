using System;
using System.Collections.Generic;
using System.Linq;
using OpenSvip.Library;
using SynthV.Model;

namespace SynthV.Param
{
    public class PitchGenerator : ParamExpression
    {
        private readonly TimeSynchronizer Synchronizer;

        private readonly ParamExpression PitchDiff;

        private readonly ParamExpression VibratoEnv;

        private readonly BaseLayerGenerator BaseLayer;

        private readonly VibratoLayerGenerator VibratoLayer;

        private readonly GaussianLayerGenerator GaussianLayer;

        public const double maxBreak = 0.01;

        public PitchGenerator(
            TimeSynchronizer synchronizer,
            List<SVNote> noteList,
            ParamExpression pitchDiff,
            ParamExpression vibratoEnv)
        {
            Synchronizer = synchronizer;
            PitchDiff = pitchDiff;
            VibratoEnv = vibratoEnv;
            if (!noteList.Any())
            {
                return;
            }

            var noteStructList = noteList.Select(note => new NoteStruct
            {
                Key = note.Pitch,
                Start = synchronizer.GetActualSecsFromTicks(TickHelper.PosToTicks(note.Onset)),
                End = synchronizer.GetActualSecsFromTicks(TickHelper.PosToTicks(note.Onset + note.Duration)),
                SlideOffset = note.Attributes.TransitionOffset,
                SlideLeft = note.Attributes.SlideLeft,
                SlideRight = note.Attributes.SlideRight,
                DepthLeft = note.Attributes.DepthLeft,
                DepthRight = note.Attributes.DepthRight,
                VibratoStart = note.Attributes.VibratoStart,
                VibratoLeft = note.Attributes.VibratoLeft,
                VibratoRight = note.Attributes.VibratoRight,
                VibratoDepth = note.Attributes.VibratoDepth,
                VibratoFrequency = note.Attributes.VibratoFrequency,
                VibratoPhase = note.Attributes.VibratoPhase
            }).ToList();
            BaseLayer = new BaseLayerGenerator(noteStructList);
            VibratoLayer = new VibratoLayerGenerator(noteStructList);
            GaussianLayer = new GaussianLayerGenerator(noteStructList);
        }

        public override int ValueAtTicks(int ticks)
        {
            return ValueAtSecs(Synchronizer.GetActualSecsFromTicks(ticks));
        }

        public int ValueAtSecs(double secs)
        {
            var ticks = (int) Math.Round(Synchronizer.GetActualTicksFromSecs(secs));
            return (int) Math.Round(BaseLayer.PitchAtSecs(secs)
                                    + GaussianLayer.PitchDiffAtSecs(secs)
                                    + VibratoLayer.PitchDiffAtSecs(secs) * VibratoEnv.ValueAtTicks(ticks) / 1000.0
                                    + PitchDiff.ValueAtTicks(ticks));
        }
    }

    internal struct NoteStruct
    {
        public int Key;
        public double Start;
        public double End;
        public double SlideOffset;
        public double SlideLeft;
        public double SlideRight;
        public double DepthLeft;
        public double DepthRight;
        public double VibratoStart;
        public double VibratoLeft;
        public double VibratoRight;
        public double VibratoDepth;
        public double VibratoFrequency;
        public double VibratoPhase;
    }

    internal class BaseLayerGenerator
    {
        private readonly struct SigmoidNode
        {
            public readonly double Start;
            public readonly double End;
            private readonly double Center;
            private readonly Func<double, double> SigmoidL;
            private readonly Func<double, double> SigmoidR;
            private readonly Func<double, double> DSigmoidL;
            private readonly Func<double, double> DSigmoidR;

            private const double K = 5.5;

            public SigmoidNode(double start, double end, double center, double radius, int keyLeft, int keyRight)
            {
                Start = start;
                End = end;
                Center = center;
                var H = (keyRight - keyLeft) * 100;
                var A = 1 / (1 + Math.Exp(K));
                const double power = 0.75;

                var L = center - start;
                double kL, hL, dL;
                if (L >= radius)
                {
                    kL = K;
                    hL = H;
                    dL = 0;
                }
                else
                {
                    var AL = A * Math.Pow(radius / L, power);
                    var BL = L / radius;
                    var CL = AL * BL * K / (2 * AL - 1);
                    kL = AL / (2 * AL - 1) * K - 1 / BL * LambertW.Evaluate(CL * Math.Exp(CL), -1);
                    hL = H * kL / (2 * kL - K);
                    dL = -radius / kL * Math.Log(2 * hL / H - 1);
                }

                SigmoidL = x => keyLeft * 100 + hL / (1 + Math.Exp(-kL / radius * (x - center + dL)));
                DSigmoidL = x =>
                {
                    var exp = Math.Exp(-kL / radius * (x - center + dL));
                    return hL * kL / radius * exp / Math.Pow(1 + exp, 2);
                };

                var R = end - center;
                double kR, hR, dR;
                if (R >= radius)
                {
                    kR = K;
                    hR = H;
                    dR = 0;
                }
                else
                {
                    var AR = A * Math.Pow(radius / R, power);
                    var BR = R / radius;
                    var CR = AR * BR * K / (2 * AR - 1);
                    kR = AR / (2 * AR - 1) * K - 1 / BR * LambertW.Evaluate(CR * Math.Exp(CR), -1);
                    hR = H * kR / (2 * kR - K);
                    dR = -radius / kR * Math.Log(2 * hR / H - 1);
                }

                SigmoidR = x => keyRight * 100 - hR / (1 + Math.Exp(-kR / radius * (center - x + dR)));
                DSigmoidR = x =>
                {
                    var exp = Math.Exp(-kR / radius * (center - x + dR));
                    return hR * kR / radius * exp / Math.Pow(1 + exp, 2);
                };
            }

            public double ValueAtSecs(double secs)
            {
                return secs <= Center ? SigmoidL(secs) : SigmoidR(secs);
            }

            public double SlopeAtSecs(double secs)
            {
                return secs <= Center ? DSigmoidL(secs) : DSigmoidR(secs);
            }
        }

        private readonly List<NoteStruct> NoteList;

        private readonly List<SigmoidNode> SigmoidNodes = new List<SigmoidNode>();

        private const double defaultRadius = 0.07;

        public BaseLayerGenerator(List<NoteStruct> noteList)
        {
            if (!noteList.Any())
            {
                return;
            }

            NoteList = noteList;

            var currentNote = NoteList[0];
            for (var i = 0; i < NoteList.Count - 1; ++i)
            {
                var nextNote = NoteList[i + 1];
                if (currentNote.Key == nextNote.Key)
                {
                    currentNote = nextNote;
                    continue;
                }
                if (nextNote.Start - currentNote.End <= PitchGenerator.maxBreak) // two notes are connected
                {
                    var start = Math.Max(currentNote.Start, Math.Min(currentNote.End,
                        currentNote.End - currentNote.SlideRight + nextNote.SlideOffset));
                    var end = Math.Max(nextNote.Start, Math.Min(nextNote.End,
                        nextNote.Start + nextNote.SlideLeft + nextNote.SlideOffset));
                    var mid = Math.Max(start, Math.Min(end,
                        (currentNote.End + nextNote.Start) / 2 + nextNote.SlideOffset));
                    SigmoidNodes.Add(new SigmoidNode(
                        start,
                        end,
                        mid,
                        (currentNote.SlideRight + nextNote.SlideLeft) / 2,
                        currentNote.Key,
                        nextNote.Key));
                }
                else
                {
                    var mid = (currentNote.End + nextNote.Start) / 2;
                    SigmoidNodes.Add(new SigmoidNode(
                        mid - defaultRadius,
                        mid + defaultRadius,
                        mid,
                        defaultRadius,
                        currentNote.Key,
                        nextNote.Key));
                }

                currentNote = nextNote;
            }
        }

        public double PitchAtSecs(double secs)
        {
            var query = SigmoidNodes.Where(node => secs >= node.Start && secs < node.End).ToArray();
            switch (query.Length)
            {
                case 0:
                    var onNoteIndex = NoteList.FindIndex(note => secs >= note.Start && secs < note.End);
                    if (onNoteIndex >= 0)
                    {
                        return 100 * NoteList[onNoteIndex].Key;
                    }

                    return NoteList
                        .OrderBy(note => Math.Min(Math.Abs(secs - note.Start), Math.Abs(secs - note.End)))
                        .First()
                        .Key * 100;
                case 1:
                    return query.First().ValueAtSecs(secs);
                case 2:
                    var first = query.First();
                    var second = query.ElementAt(1);
                    var width = first.End - second.Start;
                    var bottom = first.ValueAtSecs(second.Start);
                    var top = second.ValueAtSecs(first.End);
                    var diff1 = first.SlopeAtSecs(second.Start);
                    var diff2 = second.SlopeAtSecs(first.End);
                    return CubicBezier(width, bottom, top, diff1, diff2)(secs - second.Start);
                default:
                    throw new ArgumentException("More than two sigmoid nodes overlapped");
            }
        }

        private static Func<double, double> CubicBezier(double width, double bottom, double top, double diff1,
            double diff2)
        {
            var a = (2 * bottom - 2 * top + diff1 * width + diff2 * width) / Math.Pow(width, 3);
            var b = (-3 * bottom + 3 * top - 2 * diff1 * width - diff2 * width) / Math.Pow(width, 2);
            var c = diff1;
            var d = bottom;
            return x => a * Math.Pow(x, 3) + b * Math.Pow(x, 2) + c * x + d;
        }
    }

    internal class GaussianLayerGenerator
    {
        private readonly struct GaussianNode
        {
            public readonly double Start;
            public readonly double End;
            private readonly double Origin;
            private readonly Func<double, double> GaussianL;
            private readonly Func<double, double> GaussianR;

            private const double RatioMiu = 0.447684;
            private const double RatioSigma = 0.415;
            private const double expand = 2.5;

            public GaussianNode(
                bool isEndPoint,
                double origin,
                double depth,
                double width,
                double lengthL,
                double lengthR)
            {
                Origin = origin;
                depth *= 100;

                var sigmaBase = Math.Abs(RatioSigma * width);
                if (isEndPoint)
                {
                    var sigmaL = Math.Min(sigmaBase, lengthL / expand);
                    Start = Origin - expand * sigmaL;
                    GaussianL = x => depth * Math.Exp(-Math.Pow((x - origin) / sigmaL, 2));

                    var sigmaR = Math.Min(sigmaBase, lengthR / expand);
                    End = Origin + expand * sigmaR;
                    GaussianR = x => depth * Math.Exp(-Math.Pow((x - origin) / sigmaR, 2));
                }
                else
                {
                    var sign = Math.Sign(depth);
                    depth = Math.Abs(depth);
                    var miuBase = RatioMiu * width;
                    var R2 = Math.Pow(sigmaBase, 2);

                    var lengthBaseL = expand * sigmaBase - miuBase;
                    if (lengthL >= lengthBaseL)
                    {
                        Start = origin - lengthBaseL;
                        GaussianL = x => sign * depth * Math.Exp(-Math.Pow((x - origin - miuBase) / sigmaBase, 2));
                    }
                    else
                    {
                        Start = origin - lengthL;
                        var kL = lengthL / lengthBaseL;
                        var miuL = miuBase * kL;
                        var sigma2L = R2 * kL;
                        var depthL = depth * Math.Exp(Math.Pow(miuBase, 2) / R2 * (kL - 1));
                        GaussianL = x => sign * depthL * Math.Exp(-Math.Pow(x - origin - miuL, 2) / sigma2L);
                    }

                    var lengthBaseR = expand * sigmaBase + miuBase;
                    if (lengthR >= lengthBaseR)
                    {
                        End = origin + lengthBaseR;
                        GaussianR = x => sign * depth * Math.Exp(-Math.Pow((x - origin - miuBase) / sigmaBase, 2));
                    }
                    else
                    {
                        End = origin + lengthR;
                        var kR = lengthR / lengthBaseR;
                        var miuR = miuBase * kR;
                        var sigma2R = R2 * kR;
                        var depthR = depth * Math.Exp(Math.Pow(miuBase, 2) / R2 * (kR - 1));
                        GaussianR = x => sign * depthR * Math.Exp(-Math.Pow(x - origin - miuR, 2) / sigma2R);
                    }
                }
            }

            public double ValueAtSecs(double secs)
            {
                var value = secs < Origin ? GaussianL(secs) : GaussianR(secs);
                return value;
            }
        }

        private readonly List<GaussianNode> GaussianNodes = new List<GaussianNode>();

        public GaussianLayerGenerator(List<NoteStruct> noteList)
        {
            if (!noteList.Any())
            {
                return;
            }

            var currentNote = noteList[0];
            // head of the first note
            GaussianNodes.Add(new GaussianNode(
                true,
                currentNote.Start,
                -currentNote.DepthLeft,
                currentNote.SlideLeft,
                double.MaxValue,
                currentNote.End - currentNote.Start));
            for (var i = 0; i < noteList.Count - 1; i++)
            {
                var nextNote = noteList[i + 1];
                if (nextNote.Start - currentNote.End >= PitchGenerator.maxBreak)
                {
                    GaussianNodes.Add(new GaussianNode(
                        true,
                        currentNote.End,
                        -currentNote.DepthRight,
                        currentNote.SlideLeft,
                        currentNote.End - currentNote.Start,
                        double.MaxValue));
                    GaussianNodes.Add(new GaussianNode(
                        true,
                        nextNote.Start,
                        -nextNote.DepthLeft,
                        nextNote.SlideLeft,
                        double.MaxValue,
                        nextNote.End - nextNote.Start));
                }
                else
                {
                    var middle = (currentNote.End + nextNote.Start) / 2;
                    var origin = Math.Max(currentNote.Start, Math.Min(nextNote.End, middle + nextNote.SlideOffset));
                    
                    var depthL = currentNote.Key >= nextNote.Key
                        ? currentNote.DepthRight
                        : -currentNote.DepthRight;
                    GaussianNodes.Add(new GaussianNode(
                        false,
                        origin,
                        depthL,
                        -currentNote.SlideRight,
                        origin - currentNote.Start,
                        nextNote.End - origin));

                    var depthR = currentNote.Key >= nextNote.Key
                        ? -nextNote.DepthLeft
                        : nextNote.DepthLeft;
                    GaussianNodes.Add(new GaussianNode(
                        false,
                        origin,
                        depthR,
                        nextNote.SlideLeft,
                        origin - currentNote.Start,
                        nextNote.End - origin));
                }

                currentNote = nextNote;
            }
            // tail of the last note
            GaussianNodes.Add(new GaussianNode(
                true,
                currentNote.End,
                -currentNote.DepthRight,
                currentNote.SlideLeft,
                currentNote.End - currentNote.Start,
                double.MaxValue));
        }

        public double PitchDiffAtSecs(double secs)
        {
            return GaussianNodes
                .Where(node => secs >= node.Start && secs < node.End)
                .Sum(node => node.ValueAtSecs(secs));
        }
    }

    internal class VibratoLayerGenerator
    {
        private readonly struct VibratoNode
        {
            public readonly double Start;
            public readonly double End;
            private readonly double FadeLeft;
            private readonly double FadeRight;
            private readonly double Amplitude;
            private readonly double Frequency;
            private readonly double Phase;

            public VibratoNode(
                double start,
                double end,
                double fadeLeft,
                double fadeRight,
                double amplitude,
                double frequency,
                double phase)
            {
                Start = start;
                End = end;
                FadeLeft = fadeLeft;
                FadeRight = fadeRight;
                Amplitude = amplitude;
                Frequency = frequency;
                Phase = phase;
            }

            public double ValueAtSecs(double secs)
            {
                double zoom;
                if (End - Start >= FadeLeft + FadeLeft)
                {
                    if (secs < Start + FadeLeft)
                    {
                        zoom = (secs - Start) / FadeLeft;
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
                    var mid = (Start * FadeRight + End * FadeLeft) / (FadeLeft + FadeRight);
                    zoom = secs < mid
                        ? (secs - Start) / FadeLeft
                        : (End - secs) / FadeRight;
                }

                return zoom * Amplitude * 100.0 * Math.Sin(2 * Math.PI * Frequency * (secs - Start) + Phase);
            }
        }

        private readonly List<VibratoNode> VibratoNodes = new List<VibratoNode>();

        public VibratoLayerGenerator(List<NoteStruct> noteList)
        {
            for (var i = 0; i < noteList.Count; i++)
            {
                var start = noteList[i].Start;
                var end = noteList[i].End;
                if (end - start <= noteList[i].VibratoStart)
                {
                    continue;
                }

                if (i < noteList.Count - 1 && noteList[i + 1].Start - end < PitchGenerator.maxBreak)
                {
                    end += Math.Min(
                        noteList[i + 1].SlideOffset,
                        Math.Min(
                            noteList[i + 1].VibratoStart,
                            noteList[i + 1].End - noteList[i + 1].Start));
                }

                if (start >= end)
                {
                    continue;
                }

                VibratoNodes.Add(new VibratoNode(
                    start + noteList[i].VibratoStart,
                    end,
                    noteList[i].VibratoLeft,
                    noteList[i].VibratoRight,
                    noteList[i].VibratoDepth / 2,
                    noteList[i].VibratoFrequency,
                    noteList[i].VibratoPhase));
            }
        }

        public double PitchDiffAtSecs(double secs)
        {
            return VibratoNodes
                .Where(node => secs >= node.Start && secs < node.End)
                .Sum(node => node.ValueAtSecs(secs));
        }
    }

    internal static class TickHelper
    {
        public static int PosToTicks(long pos)
        {
            return (int) Math.Round(pos / 1470000.0);
        }
    }
}