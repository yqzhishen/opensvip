using System.Linq;
using CrSjimo.SvgPlugin.Options;
using OpenSvip.Model;

namespace CrSjimo.SvgPlugin {
    public class SvgEncoder {
        public int TrackIndex { get; set; }
        public int PixelPerBeat { get; set; }
        public int NoteHeight { get; set; }
        public int NoteRound { get; set; }
        public string NoteFillColor { get; set; }
        public string NoteStrokeColor { get; set; }
        public int NoteStrokeWidth { get; set; }
        public string PitchStrokeColor { get; set; }
        public int PitchStrokeWidth { get; set; }
        public string InnerTextColor { get; set; }
        public string SideTextColor { get; set; }
        public TextPosition LyricPosition { get; set; }
        public TextPosition PronunciationPosition { get; set; }
        public TextAlign TextAlign { get; set; }
        public SvgFactory Generate(Project project) {
            var svgFactory = new SvgFactory();
            var coordinateHelper = new CoordinateHelper {
                PixelPerBeat = PixelPerBeat,
                NoteHeight = NoteHeight,
                LyricPosition = LyricPosition,
                PronunciationPosition = PronunciationPosition,
                PitchPositionOffset = project.TimeSignatureList[0].Numerator * 480,
                TextAlign = TextAlign,
            };
            var track = (SingingTrack)project.TrackList
                .Where(trackIt => trackIt is SingingTrack)
                .ElementAt(TrackIndex);
            coordinateHelper.calculateRange(track);
            svgFactory.CoordinateHelper = coordinateHelper;
            svgFactory.ApplyStyle(
                NoteFillColor,
                NoteStrokeColor,
                NoteStrokeWidth,
                PitchStrokeColor,
                PitchStrokeWidth,
                InnerTextColor,
                SideTextColor,
                NoteRound
            );
            foreach(var note in track.NoteList) {
                svgFactory.DrawNote(note);
            }
            for(int i = 1; i < track.EditedParams.Pitch.PointList.Count - 1; i++) {
                var pitchParamPoint = track.EditedParams.Pitch.PointList[i];
                svgFactory.DrawPitch(pitchParamPoint);
            }
            return svgFactory;
        }
    }
}