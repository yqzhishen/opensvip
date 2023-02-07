using System;
using OpenSvip.Model;

using StandaloneModel = BinSvip.Standalone.Model;

namespace BinSvip.Standalone
{
    public class StandaloneSvipDecoder
    {
        public Project DecodeProject(string version, StandaloneModel.AppModel model)
        {
            var project = new Project
            {
                Version = version
            };
            foreach (var tempo in model.tempoList)
            {
                project.SongTempoList.Add(DecodeSongTempo(tempo));
            }

            foreach (var beat in model.beatList)
            {
                project.TimeSignatureList.Add(DecodeTimeSignature(beat));
            }

            foreach (var track in model.trackList)
            {
                var ele = DecodeTrack(track);
                if (ele != null)
                {
                    project.TrackList.Add(ele);
                }
            }

            return project;
        }

        private SongTempo DecodeSongTempo(StandaloneModel.SongTempo tempo)
        {
            return new SongTempo
            {
                Position = tempo.pos,
                BPM = tempo.tempo / 100.0f
            };
        }

        private TimeSignature DecodeTimeSignature(StandaloneModel.SongBeat beat)
        {
            return new TimeSignature
            {
                BarIndex = beat.barIndex,
                Numerator = beat.beatSize.x,
                Denominator = beat.beatSize.y
            };
        }

        private Track DecodeTrack(StandaloneModel.ITrack track)
        {
            Track resultTrack;
            switch (track)
            {
                case StandaloneModel.SingingTrack singingTrack:
                    var sTrack = new SingingTrack
                    {
                        AISingerName = Singers.GetName(singingTrack.AISingerId),
                        ReverbPreset = ReverbPresets.GetName(singingTrack.reverbPreset)
                    };
                    foreach (var note in singingTrack.noteList)
                    {
                        sTrack.NoteList.Add(DecodeNote(note));
                    }

                    sTrack.EditedParams = DecodeParams(singingTrack);
                    resultTrack = sTrack;
                    break;
                case StandaloneModel.InstrumentTrack instrumentTrack:
                    resultTrack = new InstrumentalTrack
                    {
                        AudioFilePath = instrumentTrack.InstrumentFilePath,
                        Offset = instrumentTrack.OffsetInPos
                    };
                    break;
                default:
                    return null;
            }

            resultTrack.Title = track.name;
            resultTrack.Mute = track.mute;
            resultTrack.Solo = track.solo;
            resultTrack.Volume = track.volume;
            resultTrack.Pan = track.pan;
            return resultTrack;
        }

        private Note DecodeNote(StandaloneModel.Note note)
        {
            var resultNote = new Note
            {
                StartPos = note.startPos,
                Length = note.widthPos,
                KeyNumber = note.keyIndex - 12,
                HeadTag = NoteHeadTags.GetName(note.headTag),
                Lyric = note.lyric
            };
            if (!"".Equals(note.pronouncing))
            {
                resultNote.Pronunciation = note.pronouncing;
            }

            var phone = note.NotePhoneInfo;
            if (phone != null)
            {
                resultNote.EditedPhones = DecodePhones(phone);
            }

            // var vibrato = note.Vibrato;
            // if (vibrato != null)
            // {
            //     resultNote.Vibrato = DecodeVibrato(note);
            // }

            return resultNote;
        }

        private Phones DecodePhones(StandaloneModel.NotePhoneInfo phone)
        {
            return new Phones
            {
                HeadLengthInSecs = phone.HeadPhoneTimeInSec,
                MidRatioOverTail = phone.MidPartOverTailPartRatio
            };
        }

        private Vibrato DecodeVibrato(StandaloneModel.Note note)
        {
            var vibrato = new Vibrato();
            var percent = note.VibratoPercentInfo;
            if (percent != null)
            {
                vibrato.StartPercent = percent.startPercent;
                vibrato.EndPercent = percent.endPercent;
            }
            else if (note.VibratoPercent > 0)
            {
                vibrato.StartPercent = 1.0f - note.VibratoPercent / 100.0f;
                vibrato.EndPercent = 1.0f;
            }

            var style = note.Vibrato;
            vibrato.IsAntiPhase = style.IsAntiPhase;
            vibrato.Amplitude = DecodeParamCurve(style.ampLine);
            vibrato.Frequency = DecodeParamCurve(style.freqLine);
            return vibrato;
        }

        private Params DecodeParams(StandaloneModel.SingingTrack track)
        {
            var @params = new Params();
            if (track.editedPitchLine != null)
            {
                @params.Pitch = DecodeParamCurve(track.editedPitchLine, op: x => x > 1050 ? x - 1150 : -100);
            }

            if (track.editedVolumeLine != null)
            {
                @params.Volume = DecodeParamCurve(track.editedVolumeLine);
            }

            if (track.editedBreathLine != null)
            {
                @params.Breath = DecodeParamCurve(track.editedBreathLine);
            }

            if (track.editedGenderLine != null)
            {
                @params.Gender = DecodeParamCurve(track.editedGenderLine);
            }

            // var powerLineProperty = track.GetType().GetProperty("EditedPowerLine");
            // if (powerLineProperty != null)
            // {
            //     var powerLine = (SingingTool.Model.Line.LineParam) powerLineProperty.GetValue(track);
            //     if (powerLine != null)
            //     {
            //         @params.Strength = DecodeParamCurve(powerLine);
            //     }
            // }
            if (track.editedPowerLine != null)
            {
                @params.Strength = DecodeParamCurve(track.editedPowerLine);
            }
            
            return @params;
        }

        private ParamCurve DecodeParamCurve(StandaloneModel.LineParam line,
            Func<int, int> op = null)
        {
            var paramCurve = new ParamCurve();
            op = op ?? (x => x);
            var point = line.nodeLinkedList.First;
            for (var i = 0; i < line.nodeLinkedList.Count; i++)
            {
                var value = point.Value;
                paramCurve.PointList.Add(new Tuple<int, int>(value.Pos, op(value.Value)));
                point = point.Next;
            }

            return paramCurve;
        }
    }
}
