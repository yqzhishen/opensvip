using System;
using OpenSvip.Const;
using OpenSvip.Model;
using Note = OpenSvip.Model.Note;

namespace OpenSvip.Stream
{
    public class BinarySvipDecoder
    {
        public Project DecodeProject(string version, SingingTool.Model.AppModel model)
        {
            var project = new Project
            {
                Version = version
            };
            foreach (var tempo in model.TempoList)
            {
                project.SongTempoList.Add(DecodeSongTempo(tempo));
            }

            foreach (var beat in model.BeatList)
            {
                project.TimeSignatureList.Add(DecodeTimeSignature(beat));
            }

            foreach (var track in model.TrackList)
            {
                var ele = DecodeTrack(track);
                if (ele != null)
                {
                    project.TrackList.Add(ele);
                }
            }
            return project;
        }
        
        private SongTempo DecodeSongTempo(SingingTool.Model.SingingGeneralConcept.SongTempo tempo)
        {
            return new SongTempo
            {
                Position = tempo.Pos,
                BPM = tempo.Tempo / 100.0f
            };
        }
        
        private TimeSignature DecodeTimeSignature(SingingTool.Model.SingingGeneralConcept.SongBeat beat)
        {
            return new TimeSignature
            {
                BarIndex = beat.BarIndex,
                Numerator = beat.BeatSize.X,
                Denominator = beat.BeatSize.Y
            };
        }
        
        private Track DecodeTrack(SingingTool.Model.ITrack track)
        {
            Track resultTrack;
            switch (track)
            {
                case SingingTool.Model.SingingTrack singingTrack:
                    var sTrack = new SingingTrack
                    {
                        AISingerName = Singers.GetName(singingTrack.AISingerId),
                        ReverbPreset = ReverbPresets.GetName(singingTrack.ReverbPreset)
                    };
                    foreach (var note in singingTrack.NoteList)
                    {
                        sTrack.NoteList.Add(DecodeNote(note));
                    }
                    sTrack.EditedParams = DecodeParams(singingTrack);
                    resultTrack = sTrack;
                    break;
                case SingingTool.Model.InstrumentTrack instrumentTrack:
                    resultTrack = new InstrumentalTrack
                    {
                        AudioFilePath = instrumentTrack.InstrumentFilePath,
                        Offset = instrumentTrack.OffsetInPos
                    };
                    break;
                default:
                    return null;
            }
            resultTrack.Title = track.Name;
            resultTrack.Mute = track.Mute;
            resultTrack.Solo = track.Solo;
            resultTrack.Volume = track.Volume;
            resultTrack.Pan = track.Pan;
            return resultTrack;
        }
        
        private Note DecodeNote(SingingTool.Model.Note note)
        {
            var resultNote = new Note
            {
                StartPos = note.ActualStartPos,
                Length = note.WidthPos,
                KeyNumber = note.KeyIndex - 12,
                HeadTag = NoteHeadTags.GetName(note.HeadTag),
                Lyric = note.Lyric
            };
            if (!"".Equals(note.Pronouncing))
            {
                resultNote.Pronunciation = note.Pronouncing;
            }
            var phone = note.NotePhoneInfo;
            if (phone != null)
            {
                resultNote.EditedPhones = DecodePhones(phone);
            }
            var vibrato = note.Vibrato;
            if (vibrato != null)
            {
                resultNote.Vibrato = DecodeVibrato(note);
            }
            return resultNote;
        }
        
        private Phones DecodePhones(SingingTool.Model.NotePhoneInfo phone)
        {
            return new Phones
            {
                HeadLengthInSecs = phone.HeadPhoneTimeInSec,
                MidRatioOverTail = phone.MidPartOverTailPartRatio
            };
        }
        
        private Vibrato DecodeVibrato(SingingTool.Model.Note note)
        {
            var vibrato = new Vibrato();
            var percent = note.VibratoPercentInfo;
            if (percent != null)
            {
                vibrato.StartPercent = percent.StartPercent;
                vibrato.EndPercent = percent.EndPercent;
            }
            else if (note.VibratoPercent > 0)
            {
                vibrato.StartPercent = 1.0f - note.VibratoPercent / 100.0f;
                vibrato.EndPercent = 1.0f;
            }
            var style = note.Vibrato;
            vibrato.IsAntiPhase = style.IsAntiPhase;
            vibrato.Amplitude = DecodeParamCurve(style.AmpLine);
            vibrato.Frequency = DecodeParamCurve(style.FreqLine);
            return vibrato;
        }
        
        private Params DecodeParams(SingingTool.Model.SingingTrack track)
        {
            var @params = new Params();
            if (track.EditedPitchLine != null)
            {
                @params.Pitch = DecodeParamCurve(track.EditedPitchLine, op: x => x > 1050 ? x - 1150 : -100);
            }
            if (track.EditedVolumeLine != null)
            {
                @params.Volume = DecodeParamCurve(track.EditedVolumeLine);
            }
            if (track.EditedBreathLine != null)
            {
                @params.Breath = DecodeParamCurve(track.EditedBreathLine);
            }
            if (track.EditedGenderLine != null)
            {
                @params.Gender = DecodeParamCurve(track.EditedGenderLine);
            }
            var powerLineProperty = track.GetType().GetProperty("EditedPowerLine");
            if (powerLineProperty != null)
            {
                var powerLine = (SingingTool.Model.Line.LineParam) powerLineProperty.GetValue(track);
                if (powerLine != null)
                {
                    @params.Strength = DecodeParamCurve(powerLine);
                }
            }
            return @params;
        }
        
        private ParamCurve DecodeParamCurve(
            SingingTool.Model.Line.LineParam line,
            Func<int, int> op = null)
        {
            var paramCurve = new ParamCurve();
            op = op ?? (x => x);
            var point = line.Begin;
            for (var i = 0; i < line.Length(); i++)
            {
                var value = point.Value;
                paramCurve.PointList.Add(new Tuple<int, int>(value.Pos, op(value.Value)));
                point = point.Next;
            }
            return paramCurve;
        }
    }
}
