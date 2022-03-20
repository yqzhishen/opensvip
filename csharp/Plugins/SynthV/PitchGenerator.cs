using System;
using System.Collections.Generic;
using System.Linq;
using OpenSvip.Library;
using OpenSvip.Model;

namespace Plugin.SynthV
{
    public class PitchGenerator
    {
        private readonly List<Note> NoteList;

        private readonly TimeSynchronizer Synchronizer;

        private readonly PitchInterpolation Interpolation;

        private readonly List<Tuple<double, int>> PitchTags = new List<Tuple<double, int>>();

        public PitchGenerator(List<Note> noteList, TimeSynchronizer synchronizer, PitchInterpolation interpolation)
        {
            NoteList = noteList;
            Synchronizer = synchronizer;
            Interpolation = interpolation;
            GenerateTags();
        }

        public double PitchAtTicks(int ticks)
        {
            return PitchAtSecs(Synchronizer.GetActualSecsFromTicks(ticks));
        }

        public double PitchAtSecs(double secs)
        {
            var index = PitchTags.FindLastIndex(tag => tag.Item1 <= secs);
            if (index == -1)
            {
                return PitchTags[0].Item2 * 100;
            }
            if (index >= PitchTags.Count - 1)
            {
                return PitchTags.Last().Item2 * 100;
            }
            if (PitchTags[index].Item2 == PitchTags[index + 1].Item2)
            {
                return PitchTags[index].Item2 * 100;
            }
            var ratio = Interpolation.Apply(
                (secs - PitchTags[index].Item1) / (PitchTags[index + 1].Item1 - PitchTags[index].Item1));
            return ((1 - ratio) * PitchTags[index].Item2 + ratio * PitchTags[index + 1].Item2) * 100;
        }

        private void GenerateTags()
        {
            if (!NoteList.Any())
            {
                return;
            }
            var maxSlideTime = Interpolation.MaxInterTimeInSecs;
            var maxSlidePercent = Interpolation.MaxInterTimePercent;
            
            var currentNote = NoteList[0];
            var currentHead = Synchronizer.GetActualSecsFromTicks(currentNote.StartPos);
            var currentDur = Synchronizer.GetDurationSecsFromTicks(
                currentNote.StartPos,
                currentNote.StartPos + currentNote.Length);
            var currentSlide = Math.Min(maxSlideTime, maxSlidePercent * currentDur);
            // head of the first note
            PitchTags.Add(new Tuple<double, int>(currentHead, currentNote.KeyNumber));
            var i = 0;
            for (; i < NoteList.Count - 1; ++i)
            {
                var nextNote = NoteList[i + 1];
                var nextHead = Synchronizer.GetActualSecsFromTicks(nextNote.StartPos);
                var nextDur = Synchronizer.GetDurationSecsFromTicks(
                    nextNote.StartPos,
                    nextNote.StartPos + nextNote.Length);
                var nextSlide = Math.Min(maxSlideTime, maxSlidePercent * nextDur);
                
                var interval = nextHead - currentHead - currentDur;
                if (interval <= 2 * maxSlideTime)
                {
                    PitchTags.Add(new Tuple<double, int>(
                        nextHead - currentSlide - interval / 2,
                        currentNote.KeyNumber));
                    PitchTags.Add(new Tuple<double, int>(
                        nextHead + nextSlide - interval / 2,
                        nextNote.KeyNumber));
                }
                else
                {
                    PitchTags.Add(new Tuple<double, int>(
                        nextHead - interval / 2 - maxSlideTime,
                        currentNote.KeyNumber));
                    PitchTags.Add(new Tuple<double, int>(
                        nextHead - interval / 2 + maxSlideTime,
                        nextNote.KeyNumber));
                }
                currentNote = nextNote;
                currentHead = nextHead;
                currentDur = nextDur;
                currentSlide = nextSlide;
            }
            // tail of the last note
            PitchTags.Add(new Tuple<double, int>(
                currentHead + currentDur,
                currentNote.KeyNumber));
        }
    }
}