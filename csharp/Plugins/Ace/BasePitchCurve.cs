using System;
using System.Collections.Generic;
using System.Linq;
using AceStdio.Model;
using AceStdio.Utils;

namespace AceStdio.Param
{
    public class BasePitchCurve
    {
        private struct NoteInSeconds
        {
            public double Start;
            public double End;
            public int Semitone;
        }
        
        private readonly double[] _valuesInSemitone;

        public BasePitchCurve(IEnumerable<AceNote> noteList, List<AceTempo> tempoList, int tickOffset = 0)
        {
            var skippedTempoList = tempoList
                .Where(tempo => tempo.Position >= tickOffset)
                .Select(
                    tempo => new AceTempo
                    {
                        Position = tempo.Position - tickOffset,
                        BPM = tempo.BPM
                    }).ToList();
            if (skippedTempoList.Any() && skippedTempoList[0].Position <= 0)
            {
                goto next;
            }
            var i = 0;
            for (; i < tempoList.Count && tempoList[i].Position <= tickOffset; i++) { }
            skippedTempoList.Insert(0, new AceTempo
            {
                Position = 0,
                BPM = tempoList[i - 1].BPM
            });
            
            next:
            var noteArray = noteList.Select(note => new NoteInSeconds
            {
                Start = TimeUtils.TickToSecond(note.Position, skippedTempoList),
                End = TimeUtils.TickToSecond(note.Position + note.Duration, skippedTempoList),
                Semitone = note.Pitch
            }).ToArray();
            _valuesInSemitone = Convolve(noteArray);
        }

        private static double[] Convolve(NoteInSeconds[] noteArray)
        {
            int totalPoints = (int)Math.Round(1000 * (noteArray[noteArray.Length - 1].End + 0.12)) + 1;
            double[] initValues = new double[totalPoints];
            int noteIndex = 0;
            for (int i = 0; i < totalPoints; i++)
            {
                initValues[i] = noteArray[noteIndex].Semitone;
                if (noteIndex < noteArray.Length - 1)
                {
                    double time = 0.001 * i;
                    if (time > 0.5 * (noteArray[noteIndex].End + noteArray[noteIndex + 1].Start))
                    {
                        noteIndex++;
                    }
                }
            }
            double[] kernel = new double[119];
            for (int i = 0; i < 119; i++)
            {
                double time = 0.001 * (i - 59);
                kernel[i] = Math.Cos(Math.PI * time / 0.12);
            }
            double sum = 0;
            for (int i = 0; i < 119; i++)
            {
                sum += kernel[i];
            }
            for (int i = 0; i < 119; i++)
            {
                kernel[i] /= sum;
            }
            double[] convolvedValues = new double[totalPoints];
            for (int i = 0; i < totalPoints; i++)
            {
                convolvedValues[i] = 0;
                for (int j = 0; j < 119; j++)
                {
                    int clippedIndex = Math.Min(Math.Max(i - 59 + j, 0), totalPoints - 1);
                    convolvedValues[i] += initValues[clippedIndex] * kernel[j];
                }
            }
            return convolvedValues;
        }

        public double SemitoneValueAt(double seconds)
        {
            double position = 1000 * Math.Max(seconds, 0);
            double leftIndex = Math.Floor(position);
            double lambda = position - leftIndex;
            int clippedLeftIndex = Math.Min((int)leftIndex, _valuesInSemitone.Length - 1);
            int clippedRightIndex = Math.Min(clippedLeftIndex + 1, _valuesInSemitone.Length - 1);
            return (1 - lambda) * _valuesInSemitone[clippedLeftIndex] + lambda * _valuesInSemitone[clippedRightIndex];
        }
    }
}