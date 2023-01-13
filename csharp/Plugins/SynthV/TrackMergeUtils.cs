using System;
using System.Collections.Generic;
using System.Linq;
using OpenSvip.Library;
using OpenSvip.Model;

namespace SynthV.Utils
{
    public static class TrackMergeUtils
    {
        public static void OverrideWith(this SingingTrack track, List<Note> noteList, Params @params, int firstBarTick)
        {
            var mainNoteList = track.NoteList;
            
            // calculate override range
            var range = OpenSvip.Library.Range.Create();
            var mainLeftIndex = -1;
            var mainRightIndex = -1;
            for (var i = 0; i < noteList.Count; i++)
            {
                mainLeftIndex = mainLeftIndex < mainNoteList.Count - 1
                    ? track.NoteList.FindLastIndex(mainLeftIndex + 1, note => note.StartPos < noteList[i].StartPos)
                    : -1;
                var mainLeftNote = mainLeftIndex >= 0 ? mainNoteList[mainLeftIndex] : null;
                var start = mainLeftNote == null || mainLeftNote.StartPos + mainLeftNote.Length <= noteList[i].StartPos - 240
                    ? noteList[i].StartPos - 120
                    : (mainLeftNote.StartPos + mainLeftNote.Length + noteList[i].StartPos) / 2;
                while (i < noteList.Count - 1 && noteList[i].StartPos + noteList[i].Length == noteList[i + 1].StartPos)
                {
                    ++i;
                }
                mainRightIndex = mainRightIndex < mainNoteList.Count - 1
                    ? track.NoteList.FindIndex(mainLeftIndex + 1, note => note.StartPos > noteList[i].StartPos)
                    : -1;
                var mainRightNote = mainRightIndex >= 0 ? mainNoteList[mainRightIndex] : null;
                var end = mainRightNote == null || mainRightNote.StartPos >= noteList[i].StartPos + noteList[i].Length + 240
                    ? noteList[i].StartPos + noteList[i].Length + 120
                    : (noteList[i].StartPos + noteList[i].Length + mainRightNote.StartPos) / 2;
                range |= OpenSvip.Library.Range.Create(new Tuple<int, int>(start, end));
            }
            
            // override notes
            track.NoteList = track.NoteList
                .Concat(noteList)
                .OrderBy(note => note.StartPos)
                .ToList();
            
            // override params
            foreach (var (start, end) in (range >> firstBarTick).SubRanges())
            {
                track.EditedParams.OverrideWith(@params, start, end);
            }
        }
        
        private static void OverrideWith(this Params mainParams, Params overrideParams, int start, int end)
        {
            mainParams.Pitch.OverrideWith(overrideParams.Pitch, start, end, -100);
            mainParams.Volume.OverrideWith(overrideParams.Volume, start, end);
            mainParams.Breath.OverrideWith(overrideParams.Breath, start, end);
            mainParams.Gender.OverrideWith(overrideParams.Gender, start, end);
            mainParams.Strength.OverrideWith(overrideParams.Strength, start, end);
        }

        private static void OverrideWith(this ParamCurve mainCurve, ParamCurve overrideCurve,
            int start, int end, int termination = 0)
        {
            var insertedPoints = new List<Tuple<int, int>>();
            var mainLeftIndex = mainCurve.PointList.FindLastIndex(point => point.Item1 < start);
            var mainRightIndex = mainCurve.PointList.FindIndex(point => point.Item1 > end);
            var overrideLeftIndex = overrideCurve.PointList.FindLastIndex(point => point.Item1 < start);
            var overrideRightIndex = overrideCurve.PointList.FindIndex(point => point.Item1 > end);
            var mainLeftDefined = mainCurve.PointList[mainLeftIndex].Item1 != termination
                                  && mainCurve.PointList[mainLeftIndex + 1].Item1 != termination;
            var overrideLeftDefined = overrideCurve.PointList[overrideLeftIndex].Item1 != termination
                                      && overrideCurve.PointList[overrideLeftIndex + 1].Item1 != termination;
            var mainRightDefined = mainCurve.PointList[mainRightIndex - 1].Item1 != termination
                                   && mainCurve.PointList[mainRightIndex].Item1 != termination;
            var overrideRightDefined = overrideCurve.PointList[overrideRightIndex - 1].Item1 != termination
                                       && overrideCurve.PointList[overrideRightIndex].Item1 != termination;
            
            if (mainLeftDefined)
            {
                var r = (double) (start - mainCurve.PointList[mainLeftIndex].Item1)
                         / (mainCurve.PointList[mainLeftIndex + 1].Item1 - mainCurve.PointList[mainLeftIndex].Item1);
                insertedPoints.Add(new Tuple<int, int>(
                    start,
                    (int) Math.Round(
                        (1 - r) * mainCurve.PointList[mainLeftIndex].Item2
                        + r * mainCurve.PointList[mainLeftIndex + 1].Item2)));
            }

            if (mainLeftDefined ^ overrideLeftDefined)
            {
                insertedPoints.Add(new Tuple<int, int>(start, termination));
            }
            if (overrideLeftDefined)
            {
                var r = (double) (start - overrideCurve.PointList[overrideLeftIndex].Item1)
                         / (overrideCurve.PointList[overrideLeftIndex + 1].Item1 - overrideCurve.PointList[overrideLeftIndex].Item1);
                insertedPoints.Add(new Tuple<int, int>(
                    start,
                    (int) Math.Round(
                        (1 - r) * overrideCurve.PointList[overrideLeftIndex].Item2
                        + r * overrideCurve.PointList[overrideLeftIndex + 1].Item2)));
            }
            
            for (var i = overrideLeftIndex + 1; i < overrideRightIndex; i++)
            {
                insertedPoints.Add(overrideCurve.PointList[i]);
            }
            
            if (overrideRightDefined)
            {
                var r = (double) (end - overrideCurve.PointList[overrideRightIndex - 1].Item1)
                         / (overrideCurve.PointList[overrideRightIndex].Item1 - overrideCurve.PointList[overrideRightIndex - 1].Item1);
                insertedPoints.Add(new Tuple<int, int>(
                    end,
                    (int) Math.Round(
                        (1 - r) * overrideCurve.PointList[overrideRightIndex - 1].Item2
                        + r * overrideCurve.PointList[overrideRightIndex].Item2)));
            }
            if (mainRightDefined ^ overrideRightDefined)
            {
                insertedPoints.Add(new Tuple<int, int>(end, termination));
            }
            if (mainRightDefined)
            {
                var r = (double) (end - mainCurve.PointList[mainRightIndex - 1].Item1)
                         / (mainCurve.PointList[mainRightIndex].Item1 - mainCurve.PointList[mainRightIndex - 1].Item1);
                insertedPoints.Add(new Tuple<int, int>(
                    end,
                    (int) Math.Round(
                        (1 - r) * mainCurve.PointList[mainRightIndex - 1].Item2
                        + r * mainCurve.PointList[mainRightIndex].Item2)));
            }
            mainCurve.PointList.RemoveRange(mainLeftIndex + 1, mainRightIndex - mainLeftIndex - 1);
            mainCurve.PointList.InsertRange(mainLeftIndex + 1, insertedPoints);
        }
    }
}
