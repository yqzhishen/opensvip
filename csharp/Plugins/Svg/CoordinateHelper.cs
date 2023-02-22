using System;
using CrSjimo.SvgPlugin.Options;
using OpenSvip.Model;

namespace CrSjimo.SvgPlugin {
    public class CoordinateHelper {
        public const int TICKS_PER_BEAT = 480;
        public const int PADDING = 4;
        public int PixelPerBeat { get; set; }
        public int NoteHeight { get; set; }
        public TextPosition LyricPosition { get; set; }
        public TextPosition PronunciationPosition { get; set; }
        public int PitchPositionOffset;
        private int PositionRangeStart;
        private int PositionRangeEnd;
        private int KeyRangeStart;
        private int KeyRangeEnd;
        public void calculateRange(SingingTrack track) {
            PositionRangeStart = Math.Min(
                track.NoteList[0].StartPos,
                track.EditedParams.Pitch[1].Item1
            );
            PositionRangeEnd = Math.Max(
                track.NoteList[track.NoteList.Count - 1].StartPos + track.NoteList[track.NoteList.Count - 1].Length,
                track.EditedParams.Pitch.PointList[track.EditedParams.Pitch.PointList.Count - 2].Item1
            );
            KeyRangeStart = int.MaxValue;
            KeyRangeEnd = int.MinValue;
            foreach(var note in track.NoteList) {
                KeyRangeStart = Math.Min(KeyRangeStart, note.KeyNumber);
                KeyRangeEnd = Math.Max(KeyRangeEnd, note.KeyNumber);
            }
            for(int i = 1; i < track.EditedParams.Pitch.PointList.Count - 1; i++) {
                var paramPoint = track.EditedParams.Pitch.PointList[i];
                if(paramPoint.Item2 == -100) continue;
                KeyRangeStart = Math.Min(KeyRangeStart, (int)Math.Floor((double)paramPoint.Item2/100));
                KeyRangeEnd = Math.Max(KeyRangeEnd, (int)Math.Ceiling((double)paramPoint.Item2/100));
            }
        }
        
        public NotePositionParameters GetNotePositionParameters(Note note) {
            return new NotePositionParameters {
                Point1 = new Tuple<double, double>(
                    1.0 * (note.StartPos - PositionRangeStart) * PixelPerBeat / TICKS_PER_BEAT,
                    (KeyRangeEnd - note.KeyNumber) * NoteHeight
                ),
                Point2 = new Tuple<double, double>(
                    1.0 * (note.StartPos + note.Length - PositionRangeStart) * PixelPerBeat / TICKS_PER_BEAT,
                    (KeyRangeEnd - note.KeyNumber + 1) * NoteHeight
                ),
                TextSize = NoteHeight - 2 * PADDING,
                InnerText = new Tuple<double, double>(
                    1.0 * (note.StartPos - PositionRangeStart) * PixelPerBeat / TICKS_PER_BEAT + PADDING,
                    (KeyRangeEnd - note.KeyNumber + 1) * NoteHeight - PADDING
                ),
                UpperText = new Tuple<double, double>(
                    1.0 * (note.StartPos - PositionRangeStart) * PixelPerBeat / TICKS_PER_BEAT,
                    (KeyRangeEnd - note.KeyNumber) * NoteHeight - PADDING
                ),
                LowerText = new Tuple<double, double>(
                    1.0 * (note.StartPos - PositionRangeStart) * PixelPerBeat / TICKS_PER_BEAT,
                    (KeyRangeEnd - note.KeyNumber + 2) * NoteHeight - PADDING
                ),
            };
        }
        public Tuple<double, double> getPitchPoint(Tuple<int, int> paramPoint) {
            return new Tuple<double, double>(
                1.0 * (paramPoint.Item1 - PositionRangeStart - PitchPositionOffset) * PixelPerBeat / TICKS_PER_BEAT,
                (KeyRangeEnd - paramPoint.Item2 / 100.0 + 0.5) * NoteHeight
            );
        }
        public Tuple<double, double> getSize() {
            return new Tuple<double, double>(
                1.0 * (PositionRangeEnd - PositionRangeStart) * PixelPerBeat / TICKS_PER_BEAT,
                (KeyRangeEnd - KeyRangeStart + 1) * NoteHeight
            );
        }
        public int getFontSize() {
            return NoteHeight - 2 * PADDING;
        }
    }
    public class NotePositionParameters {
        public Tuple<double, double> Point1, Point2, InnerText, UpperText, LowerText;
        public double TextSize;
    }
}