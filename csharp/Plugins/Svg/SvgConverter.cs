using CrSjimo.SvgPlugin.Options;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;

namespace CrSjimo.SvgPlugin.Stream {
    internal class SvgConverter: IProjectConverter {
        public Project Load(string path, ConverterOptions options) {
            throw new NotImplementedException("不支持将本插件作为输入端。");
        }
        public void Save(string path, Project project, ConverterOptions options) {
            var svgFactory = new SvgEncoder {
                PixelPerBeat = options.GetValueAsInteger("PixelPerBeat", 48),
                NoteHeight = options.GetValueAsInteger("NoteHeight", 24),
                NoteRound = options.GetValueAsInteger("NoteRound", 4),
                NoteFillColor = options.GetValueAsString("NoteFillColor", "#CCFFCC"),
                NoteStrokeColor = options.GetValueAsString("NoteStrokeColor", "#006600"),
                NoteStrokeWidth = options.GetValueAsInteger("NoteStrokeWidth", 1),
                PitchStrokeColor = options.GetValueAsString("PitchStrokeColor", "#99aa99"),
                PitchStrokeWidth = options.GetValueAsInteger("PitchStrokeWidth", 2),
                InnerTextColor = options.GetValueAsString("InnerTextColor", "#000000"),
                SideTextColor = options.GetValueAsString("SideTextColor", "#000000"),
                LyricPosition = options.GetValueAsEnum("LyricPosition", TextPosition.Lower),
                PronunciationPosition = options.GetValueAsEnum("PronunciationPosition", TextPosition.Inner),
            }.Generate(project);
            svgFactory.Write(path);
        }
    }
}