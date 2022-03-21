using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using OpenSvip.Const;
using OpenSvip.Library;
using OpenSvip.Model;

namespace OpenSvip.Stream
{
    public class BinarySvipEncoder
    {
        public string DefaultSinger { get; set; }
        
        public int DefaultTempo { get; set; }

        private bool IsAbsoluteTimeMode;

        private TimeSynchronizer Synchronizer;

        private int FirstBarTick;

        private List<SongTempo> FirstBarTempo;

        public Tuple<string, SingingTool.Model.AppModel> EncodeProject(Project project)
        {
            var version = Regex.IsMatch(project.Version, @"^SVIP\d\.\d\.\d$")
                ? project.Version
                : "SVIP" + SingingTool.Const.ToolConstValues.ProjectVersion;
            var model = new SingingTool.Model.AppModel();
            FirstBarTick = (int) Math.Round(1920.0 * project.TimeSignatureList[0].Numerator / project.TimeSignatureList[0].Denominator);
            FirstBarTempo = project.SongTempoList.Where(tempo => tempo.Position < FirstBarTick).ToList();
            IsAbsoluteTimeMode = project.SongTempoList.Any(tempo => tempo.BPM < 20 || tempo.BPM > 300);
            Synchronizer = new TimeSynchronizer(project.SongTempoList, FirstBarTick, IsAbsoluteTimeMode, DefaultTempo);
            
            // beat
            if (IsAbsoluteTimeMode || project.TimeSignatureList.Any(beat => beat.Numerator > 255 || beat.Denominator > 32))
            {
                model.BeatList.InsertItemInOrder(new SingingTool.Model.SingingGeneralConcept.SongBeat
                {
                    BarIndex = 0,
                    BeatSize = new SingingTool.Model.SingingGeneralConcept.BeatSize
                    {
                        X = 4,
                        Y = 4
                    }
                });
            }
            else
            {
                foreach (var beat in project.TimeSignatureList)
                {
                    model.BeatList.InsertItemInOrder(EncodeTimeSignature(beat));
                }
            }
            
            // tempo
            if (IsAbsoluteTimeMode)
            {
                model.TempoList.InsertItemInOrder(new SingingTool.Model.SingingGeneralConcept.SongTempo
                {
                    Pos = 0,
                    Tempo = DefaultTempo * 100
                });
            }
            else
            {
                foreach (var tempo in project.SongTempoList)
                {
                    model.TempoList.InsertItemInOrder(EncodeSongTempo(tempo));
                }
                
            }
            
            // tracks
            foreach (var t in project.TrackList.Select(EncodeTrack).Where(t => t != null))
            {
                model.TrackList.Add(t);
            }
            return new Tuple<string, SingingTool.Model.AppModel>(version, model);
        }
        
        private SingingTool.Model.SingingGeneralConcept.SongTempo EncodeSongTempo(SongTempo tempo)
        {
            return new SingingTool.Model.SingingGeneralConcept.SongTempo
            {
                Pos = tempo.Position,
                Tempo = (int) Math.Round(tempo.BPM * 100)
            };
        }
        
        private SingingTool.Model.SingingGeneralConcept.SongBeat EncodeTimeSignature(TimeSignature signature)
        {
            return new SingingTool.Model.SingingGeneralConcept.SongBeat
            {
                BarIndex = signature.BarIndex,
                BeatSize = new SingingTool.Model.SingingGeneralConcept.BeatSize
                {
                    X = signature.Numerator,
                    Y = signature.Denominator
                }
            };
        }

        private SingingTool.Model.ITrack EncodeTrack(Track track)
        {
            SingingTool.Model.ITrack resultTrack;
            switch (track)
            {
                case SingingTrack singingTrack:
                    var sTrack = new SingingTool.Model.SingingTrack
                    {
                        AISingerId = Singers.GetId(singingTrack.AISingerName, DefaultSinger),
                        ReverbPreset = ReverbPresets.GetIndex(singingTrack.ReverbPreset)
                    };
                    foreach (var note in singingTrack.NoteList)
                    {
                        sTrack.NoteList.InsertItemInOrder(EncodeNote(note));
                    }
                    var paramDict = EncodeParams(singingTrack.EditedParams);
                    sTrack.EditedPitchLine = paramDict["Pitch"];
                    sTrack.EditedVolumeLine = paramDict["Volume"];
                    sTrack.EditedBreathLine = paramDict["Breath"];
                    sTrack.EditedGenderLine = paramDict["Gender"];
                    sTrack.EditedPowerLine = paramDict["Strength"];
                    resultTrack = sTrack;
                    break;
                case InstrumentalTrack instrumentalTrack:
                    resultTrack = new SingingTool.Model.InstrumentTrack
                    {
                        InstrumentFilePath = instrumentalTrack.AudioFilePath,
                        OffsetInPos = EncodeAudioOffset(instrumentalTrack.Offset)
                    };
                    break;
                default:
                    return null;
            }
            resultTrack.Name = track.Title;
            resultTrack.Mute = track.Mute;
            resultTrack.Solo = track.Solo;
            resultTrack.Volume = track.Volume;
            resultTrack.Pan = track.Pan;
            return resultTrack;
        }
        
        private SingingTool.Model.Note EncodeNote(Note note)
        {
            var resultNote = new SingingTool.Model.Note
            {
                ActualStartPos = (int) Math.Round(Synchronizer.GetActualTicksFromTicks(note.StartPos)),
                KeyIndex = note.KeyNumber + 12,
                Lyric = note.Lyric,
                HeadTag = NoteHeadTags.GetIndex(note.HeadTag),
                Pronouncing = note.Pronunciation
            };
            resultNote.WidthPos = (int) Math.Round(
                    Synchronizer.GetActualTicksFromTicks(note.StartPos + note.Length)) - resultNote.ActualStartPos;
            if (note.EditedPhones != null)
            {
                resultNote.NotePhoneInfo = EncodePhones(note.EditedPhones);
            }
            if (note.Vibrato == null)
            {
                return resultNote;
            }
            var (percent, vibrato) = EncodeVibrato(note.Vibrato);
            resultNote.VibratoPercentInfo = percent;
            resultNote.Vibrato = vibrato;
            return resultNote;
        }
        
        private SingingTool.Model.NotePhoneInfo EncodePhones(Phones phones)
        {
            return new SingingTool.Model.NotePhoneInfo
            {
                HeadPhoneTimeInSec = phones.HeadLengthInSecs,
                MidPartOverTailPartRatio = phones.MidRatioOverTail
            };
        }
        
        private Tuple<SingingTool.Model.VibratoPercentInfo, SingingTool.Model.VibratoStyle>
            EncodeVibrato(Vibrato vibrato)
        {
            var percent = new SingingTool.Model.VibratoPercentInfo(vibrato.StartPercent, vibrato.EndPercent);
            var style = new SingingTool.Model.VibratoStyle
            {
                IsAntiPhase = vibrato.IsAntiPhase
            };
            const BindingFlags flag = BindingFlags.Instance | BindingFlags.Public;
            var type = style.GetType();
            var ampLine = type.GetProperty("AmpLine", flag);
            var freqLine = type.GetProperty("FreqLine", flag);
            ampLine?.SetValue(style, EncodeParamCurve(vibrato.Amplitude, left: -1, right: 100001, isTicks: false), null);
            freqLine?.SetValue(style, EncodeParamCurve(vibrato.Frequency, left: -1, right: 100001, isTicks: false), null);
            return new Tuple<SingingTool.Model.VibratoPercentInfo, SingingTool.Model.VibratoStyle>(percent, style);
        }
        
        private Dictionary<string, SingingTool.Model.Line.LineParam> EncodeParams(Params @params)
        {
            return new Dictionary<string, SingingTool.Model.Line.LineParam>
            {
                {"Pitch", EncodeParamCurve(@params.Pitch, op: x => x > -100 ? x + 1150 : -100, termination: -100)},
                {"Volume", EncodeParamCurve(@params.Volume)},
                {"Breath", EncodeParamCurve(@params.Breath)},
                {"Gender", EncodeParamCurve(@params.Gender)},
                {"Strength", EncodeParamCurve(@params.Strength)}
            };
        }

        private SingingTool.Model.Line.LineParam EncodeParamCurve(
            ParamCurve paramCurve,
            Func<int, int> op = null,
            int left = -192000,
            int right = 1073741823,
            int termination = 0,
            bool isTicks = true)
        {
            op = op ?? (x => x);
            var line = new SingingTool.Model.Line.LineParam();
            var length = paramCurve.PointList.Count;
            foreach (var (pos, value) in paramCurve.PointList
                         .Where(point => point.Item1 >= left && point.Item1 <= right)
                         .OrderBy(point => point.Item1))
            {
                var actualPos = IsAbsoluteTimeMode && isTicks && pos != left && pos != right
                    ? (int) Math.Round(Synchronizer.GetActualTicksFromTicks(pos - FirstBarTick) + 1920)
                    : pos;
                line.PushBack(new SingingTool.Model.Line.LineParamNode(actualPos, op(value)));
            }
            if (length == 0 || line.Begin.Value.Pos > left)
            {
                line.PushFront(new SingingTool.Model.Line.LineParamNode(left, termination));
            }
            if (length == 0 || line.Back.Pos < right)
            {
                line.PushBack(new SingingTool.Model.Line.LineParamNode(right, termination));
            }
            paramCurve.TotalPointsCount = line.Length();
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
                return (int) Math.Round(Synchronizer.GetActualTicksFromTicks(offset));
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
            return (int) Math.Round(res);
        }
    }
}
