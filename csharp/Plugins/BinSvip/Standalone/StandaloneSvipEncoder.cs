using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using OpenSvip.Library;
using OpenSvip.Model;
using XStudio = BinSvip.Standalone.Model;

namespace BinSvip.Standalone
{
    public class StandaloneSvipEncoder
    {
        public string DefaultSinger { get; set; }

        public int DefaultTempo { get; set; }

        private bool IsAbsoluteTimeMode;

        private TimeSynchronizer Synchronizer;

        private int FirstBarTick;

        private List<SongTempo> FirstBarTempo;

        public Tuple<string, XStudio.AppModel> EncodeProject(Project project)
        {
            var version = Regex.IsMatch(project.Version, @"^SVIP\d\.\d\.\d$")
                ? project.Version
                : "SVIP" + "6.0.0";
            var model = new XStudio.AppModel();
            FirstBarTick = (int)Math.Round(1920.0 * project.TimeSignatureList[0].Numerator /
                                           project.TimeSignatureList[0].Denominator);
            FirstBarTempo = project.SongTempoList.Where(tempo => tempo.Position < FirstBarTick).ToList();
            IsAbsoluteTimeMode = project.SongTempoList.Any(tempo => tempo.BPM < 20 || tempo.BPM > 300);
            Synchronizer = new TimeSynchronizer(project.SongTempoList, FirstBarTick, IsAbsoluteTimeMode, DefaultTempo);

            // beat
            if (IsAbsoluteTimeMode ||
                project.TimeSignatureList.Any(beat => beat.Numerator > 255 || beat.Denominator > 32))
            {
                model.beatList.Add(new XStudio.SongBeat
                {
                    barIndex = 0,
                    beatSize = new XStudio.BeatSize
                    {
                        x = 4,
                        y = 4
                    }
                });
            }
            else
            {
                foreach (var beat in project.TimeSignatureList)
                {
                    model.beatList.Add(EncodeTimeSignature(beat));
                }
            }

            // tempo
            if (IsAbsoluteTimeMode)
            {
                model.tempoList.Add(new XStudio.SongTempo
                {
                    pos = 0,
                    tempo = DefaultTempo * 100
                });
            }
            else
            {
                foreach (var tempo in project.SongTempoList)
                {
                    model.tempoList.Add(EncodeSongTempo(tempo));
                }
            }

            // tracks
            foreach (var t in project.TrackList.Select(EncodeTrack).Where(t => t != null))
            {
                model.trackList.Add(t);
            }

            return new Tuple<string, XStudio.AppModel>(version, model);
        }

        private XStudio.SongTempo EncodeSongTempo(SongTempo tempo)
        {
            return new XStudio.SongTempo
            {
                pos = tempo.Position,
                tempo = (int)Math.Round(tempo.BPM * 100)
            };
        }

        private XStudio.SongBeat EncodeTimeSignature(TimeSignature signature)
        {
            return new XStudio.SongBeat
            {
                barIndex = signature.BarIndex,
                beatSize = new XStudio.BeatSize
                {
                    x = signature.Numerator,
                    y = signature.Denominator
                }
            };
        }

        private XStudio.ITrack EncodeTrack(Track track)
        {
            XStudio.ITrack resultTrack;
            switch (track)
            {
                case SingingTrack singingTrack:
                    var singerId = Singers.GetId(singingTrack.AISingerName);
                    if (singerId == "")
                    {
                        singerId = Singers.GetId(DefaultSinger);
                    }

                    var sTrack = new XStudio.SingingTrack
                    {
                        AISingerId = singerId,
                        reverbPreset = ReverbPresets.GetIndex(singingTrack.ReverbPreset)
                    };
                    foreach (var note in singingTrack.NoteList)
                    {
                        sTrack.noteList.Add(EncodeNote(note));
                    }

                    var paramDict = EncodeParams(singingTrack.EditedParams);
                    sTrack.editedPitchLine = paramDict["Pitch"];
                    sTrack.editedVolumeLine = paramDict["Volume"];
                    sTrack.editedBreathLine = paramDict["Breath"];
                    sTrack.editedGenderLine = paramDict["Gender"];
                    var powerLineProperty = sTrack.GetType().GetProperty("EditedPowerLine");
                    if (powerLineProperty != null)
                    {
                        powerLineProperty.SetValue(sTrack, paramDict["Strength"]);
                    }

                    resultTrack = sTrack;
                    break;
                case InstrumentalTrack instrumentalTrack:
                    resultTrack = new XStudio.InstrumentTrack
                    {
                        InstrumentFilePath = instrumentalTrack.AudioFilePath,
                        OffsetInPos = EncodeAudioOffset(instrumentalTrack.Offset)
                    };
                    break;
                default:
                    return null;
            }

            resultTrack.name = track.Title;
            resultTrack.mute = track.Mute;
            resultTrack.solo = track.Solo;
            resultTrack.volume = track.Volume;
            resultTrack.pan = track.Pan;
            return resultTrack;
        }

        private XStudio.Note EncodeNote(Note note)
        {
            var resultNote = new XStudio.Note
            {
                startPos = (int)Math.Round(Synchronizer.GetActualTicksFromTicks(note.StartPos)),
                keyIndex = note.KeyNumber + 12,
                lyric = note.Lyric,
                headTag = NoteHeadTags.GetIndex(note.HeadTag),
                pronouncing = note.Pronunciation?.ToLower()
            };
            resultNote.widthPos = (int)Math.Round(
                Synchronizer.GetActualTicksFromTicks(note.StartPos + note.Length)) - resultNote.startPos;
            if (note.EditedPhones != null)
            {
                resultNote.NotePhoneInfo = EncodePhones(note.EditedPhones);
            }

            // if (note.Vibrato == null)
            // {
            //     return resultNote;
            // }
            //
            // var (percent, vibrato) = EncodeVibrato(note.Vibrato);
            // resultNote.VibratoPercentInfo = percent;
            // resultNote.Vibrato = vibrato;
            return resultNote;
        }

        private XStudio.NotePhoneInfo EncodePhones(Phones phones)
        {
            return new XStudio.NotePhoneInfo
            {
                HeadPhoneTimeInSec = phones.HeadLengthInSecs,
                MidPartOverTailPartRatio = phones.MidRatioOverTail
            };
        }

        private Tuple<XStudio.VibratoPercentInfo, XStudio.VibratoStyle>
            EncodeVibrato(Vibrato vibrato)
        {
            var percent = new XStudio.VibratoPercentInfo(vibrato.StartPercent, vibrato.EndPercent);
            var style = new XStudio.VibratoStyle
            {
                IsAntiPhase = vibrato.IsAntiPhase
            };
            const BindingFlags flag = BindingFlags.Instance | BindingFlags.Public;
            var type = style.GetType();
            var ampLine = type.GetProperty("AmpLine", flag);
            var freqLine = type.GetProperty("FreqLine", flag);
            ampLine?.SetValue(style, EncodeParamCurve(vibrato.Amplitude, left: -1, right: 100001, isTicks: false),
                null);
            freqLine?.SetValue(style, EncodeParamCurve(vibrato.Frequency, left: -1, right: 100001, isTicks: false),
                null);
            return new Tuple<XStudio.VibratoPercentInfo, XStudio.VibratoStyle>(percent, style);
        }

        private Dictionary<string, XStudio.LineParam> EncodeParams(Params @params)
        {
            return new Dictionary<string, XStudio.LineParam>
            {
                { "Pitch", EncodeParamCurve(@params.Pitch, op: x => x > -100 ? x + 1150 : -100, termination: -100) },
                { "Volume", EncodeParamCurve(@params.Volume) },
                { "Breath", EncodeParamCurve(@params.Breath) },
                { "Gender", EncodeParamCurve(@params.Gender) },
                { "Strength", EncodeParamCurve(@params.Strength) }
            };
        }

        private XStudio.LineParam EncodeParamCurve(
            ParamCurve paramCurve,
            Func<int, int> op = null,
            int left = -192000,
            int right = 1073741823,
            int termination = 0,
            bool isTicks = true)
        {
            op = op ?? (x => x);
            var line = new XStudio.LineParam();
            var length = paramCurve.PointList.Count;
            foreach (var (pos, value) in paramCurve.PointList
                         .Where(point => point.Item1 >= left && point.Item1 <= right)
                         .OrderBy(point => point.Item1))
            {
                var actualPos = IsAbsoluteTimeMode && isTicks && pos != left && pos != right
                    ? (int)Math.Round(Synchronizer.GetActualTicksFromTicks(pos - FirstBarTick) + 1920)
                    : pos;
                line.nodeLinkedList.AddLast(new XStudio.LineParamNode(actualPos, op(value)));
            }

            if (length == 0 || line.nodeLinkedList.First.Value.Pos > left)
            {
                line.nodeLinkedList.AddFirst(new XStudio.LineParamNode(left, termination));
            }

            if (length == 0 || line.nodeLinkedList.Last.Value.Pos < right)
            {
                line.nodeLinkedList.AddLast(new XStudio.LineParamNode(right, termination));
            }

            return line;
        }

        private int EncodeAudioOffset(int offset)
        {
            if (!IsAbsoluteTimeMode)
            {
                return offset;
            }

            if (offset > 0)
            {
                return (int)Math.Round(Synchronizer.GetActualTicksFromTicks(offset));
            }

            var currentPos = FirstBarTick;
            var actualPos = FirstBarTick + offset;
            var res = 0.0;
            var i = FirstBarTempo.Count - 1;
            for (; i >= 0 && actualPos <= FirstBarTempo[i].Position; i--)
            {
                res -= (currentPos - FirstBarTempo[i].Position) * DefaultTempo / FirstBarTempo[i].BPM;
                currentPos = FirstBarTempo[i].Position;
            }

            if (i >= 0)
            {
                res -= (currentPos - actualPos) * DefaultTempo / FirstBarTempo[i].BPM;
            }
            else
            {
                res += actualPos * DefaultTempo / FirstBarTempo[0].BPM;
            }

            return (int)Math.Round(res);
        }
    }
}
