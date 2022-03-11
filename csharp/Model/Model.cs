using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenSvip.Const;
using OpenSvip.Serialization;

namespace OpenSvip.Model;

public class Project
{
    public string Version { get; set; } = "";

    public List<SongTempo> SongTempoList { get; set; } = new();

    public List<TimeSignature> TimeSignatureList { get; set; } = new();

    public List<Track> TrackList { get; set; } = new();

    public Project Decode(string version, SingingTool.Model.AppModel model)
    {
        Version = version;
        foreach (var tempo in model.TempoList)
        {
            SongTempoList.Add(new SongTempo().Decode(tempo));
        }

        foreach (var beat in model.BeatList)
        {
            TimeSignatureList.Add(new TimeSignature().Decode(beat));
        }

        foreach (var track in model.TrackList)
        {
            switch (track)
            {
                case SingingTool.Model.SingingTrack singingTrack:
                    TrackList.Add(new SingingTrack().Decode(singingTrack));
                    break;
                case SingingTool.Model.InstrumentTrack instrumentalTrack:
                    TrackList.Add(new InstrumentalTrack().Decode(instrumentalTrack));
                    break;
            }
        }
        return this;
    }

    public Tuple<string, SingingTool.Model.AppModel> Encode()
    {
        var model = new SingingTool.Model.AppModel();
        foreach (var tempo in SongTempoList)
        {
            model.TempoList.InsertItemInOrder(tempo.Encode());
        }
        foreach (var beat in TimeSignatureList)
        {
            model.BeatList.InsertItemInOrder(beat.Encode());
        }
        foreach (var t in TrackList.Select(track => track.Encode()).Where(t => t != null))
        {
            model.TrackList.Add(t);
        }
        return new Tuple<string, SingingTool.Model.AppModel>(Version, model);
    }
}

public class SongTempo
{
    public int Position { get; set; }
    public double BPM { get; set; }

    public SongTempo Decode(SingingTool.Model.SingingGeneralConcept.SongTempo tempo)
    {
        Position = tempo.Pos;
        BPM = tempo.Tempo / 100.0;
        return this;
    }

    public SingingTool.Model.SingingGeneralConcept.SongTempo Encode()
    {
        return new SingingTool.Model.SingingGeneralConcept.SongTempo
        {
            Pos = Position,
            Tempo = (int) Math.Round(BPM * 100)
        };
    }
}

public class TimeSignature
{
    public int BarIndex { get; set; }
    public int Numerator { get; set; }
    public int Denominator { get; set; }

    public TimeSignature Decode(SingingTool.Model.SingingGeneralConcept.SongBeat beat)
    {
        BarIndex = beat.BarIndex;
        Numerator = beat.BeatSize.X;
        Denominator = beat.BeatSize.Y;
        return this;
    }

    public SingingTool.Model.SingingGeneralConcept.SongBeat Encode()
    {
        return new SingingTool.Model.SingingGeneralConcept.SongBeat
        {
            BarIndex = BarIndex,
            BeatSize = new SingingTool.Model.SingingGeneralConcept.BeatSize
            {
                X = Numerator,
                Y = Denominator
            }
        };
    }
}

[JsonConverter(typeof(TrackJsonConverter))]
public class Track
{
    public readonly string Type;
    public string Title { get; set; } = "";
    public bool Muted { get; set; }
    public bool Solo { get; set; }
    public double Volume { get; set; }
    public double Pan { get; set; }

    protected Track(string type)
    {
        Type = type;
    }

    protected Track Decode(SingingTool.Model.ITrack track)
    {
        Title = track.Name;
        Muted = track.Mute;
        Solo = track.Solo;
        Volume = track.Volume;
        Pan = track.Pan;
        return this;
    }

    public virtual SingingTool.Model.ITrack Encode()
    {
        SingingTool.Model.ITrack track;
        switch (Type)
        {
            case "Singing":
                track = new SingingTool.Model.SingingTrack();
                break;
            case "Instrumental":
                track = new SingingTool.Model.InstrumentTrack();
                break;
            default:
                return null;
        }
        track.Name = Title;
        track.Mute = Muted;
        track.Solo = Solo;
        track.Volume = Volume;
        track.Pan = Pan;
        return track;
    }
}

[JsonConverter(typeof(TrackJsonConverter))]
public class SingingTrack: Track
{
    public string AISingerName { get; set; } = "";
    public string ReverbPreset { get; set; } = "";
    public List<Note> NoteList { get; set; } = new();
    public Params EditedParams { get; set; } = new();

    public SingingTrack() : base("Singing") { }

    public SingingTrack Decode(SingingTool.Model.SingingTrack track)
    {
        base.Decode(track);
        AISingerName = Singers.GetName(track.AISingerId);
        ReverbPreset = ReverbPresets.GetName(track.ReverbPreset);
        Console.Write(ReverbPreset + ": " + track.ReverbPreset);
        foreach (var note in track.NoteList)
        {
            NoteList.Add(new Note().Decode(note));
        }
        EditedParams.Decode(track);
        return this;
    }

    public override SingingTool.Model.ITrack Encode()
    {
        var track = (SingingTool.Model.SingingTrack) base.Encode();
        track.AISingerId = Singers.GetId(AISingerName);
        track.ReverbPreset = ReverbPresets.GetIndex(ReverbPreset);
        foreach (var note in NoteList)
        {
            track.NoteList.InsertItemInOrder(note.Encode());
        }
        var paramTable = EditedParams.Encode();
        track.EditedPitchLine = (SingingTool.Model.Line.LineParam) paramTable["Pitch"];
        track.EditedVolumeLine = (SingingTool.Model.Line.LineParam) paramTable["Volume"];
        track.EditedBreathLine = (SingingTool.Model.Line.LineParam) paramTable["Breath"];
        track.EditedGenderLine = (SingingTool.Model.Line.LineParam) paramTable["Gender"];
        track.EditedPowerLine = (SingingTool.Model.Line.LineParam) paramTable["Strength"];
        return track;
    }
}

[JsonConverter(typeof(TrackJsonConverter))]
public class InstrumentalTrack: Track
{
    public string AudioFilePath { get; set; } = "";
    public int Offset { get; set; }
    
    public InstrumentalTrack() : base("Instrumental") { }

    public InstrumentalTrack Decode(SingingTool.Model.InstrumentTrack track)
    {
        base.Decode(track);
        AudioFilePath = track.InstrumentFilePath;
        Offset = track.OffsetInPos;
        return this;
    }

    public override SingingTool.Model.ITrack Encode()
    {
        var track = (SingingTool.Model.InstrumentTrack) base.Encode();
        track.InstrumentFilePath = AudioFilePath;
        track.OffsetInPos = Offset;
        return track;
    }
}

public class Note
{
    public int StartPos { get; set; }
    public int Length { get; set; }
    public int KeyNumber { get; set; }
    public string HeadTag { get; set; }
    public string Lyric { get; set; } = "";
    public string Pronunciation { get; set; }
    public Phones EditedPhones { get; set; }
    public Vibrato Vibrato { get; set; }

    public Note Decode(SingingTool.Model.Note note)
    {
        StartPos = note.StartPos;
        Length = note.WidthPos;
        KeyNumber = note.KeyIndex - 12;
        HeadTag = NoteHeadTags.GetName(note.HeadTag);
        Lyric = note.Lyric;
        if (!"".Equals(note.Pronouncing))
        {
            Pronunciation = note.Pronouncing;
        }
        var phone = note.NotePhoneInfo;
        if (phone != null)
        {
            EditedPhones = new Phones().Decode(phone);
        }
        var vibrato = note.Vibrato;
        if (vibrato != null)
        {
            Vibrato = new Vibrato().Decode(note);
        }
        return this;
    }

    public SingingTool.Model.Note Encode()
    {
        var note = new SingingTool.Model.Note(StartPos, Length, KeyNumber + 12, Lyric)
        {
            HeadTag = NoteHeadTags.GetIndex(HeadTag),
            Pronouncing = Pronunciation
        };
        if (EditedPhones != null)
        {
            note.NotePhoneInfo = EditedPhones.Encode();
        }
        if (Vibrato == null)
        {
            return note;
        }
        var (percent, vibrato) = Vibrato.Encode();
        note.VibratoPercentInfo = percent;
        note.Vibrato = vibrato;
        return note;
    }
}

public class Phones
{
    public float HeadLengthInSecs { get; set; } = -1.0f;
    public float MidRatioOverTail { get; set; } = -1.0f;

    public Phones Decode(SingingTool.Model.NotePhoneInfo phone)
    {
        HeadLengthInSecs = phone.HeadPhoneTimeInSec;
        MidRatioOverTail = phone.MidPartOverTailPartRatio;
        return this;
    }

    public SingingTool.Model.NotePhoneInfo Encode()
    {
        return new SingingTool.Model.NotePhoneInfo
        {
            HeadPhoneTimeInSec = HeadLengthInSecs,
            MidPartOverTailPartRatio = MidRatioOverTail
        };
    }
}

public class Vibrato
{
    public float StartPercent { get; set; }
    public float EndPercent { get; set; }
    public bool IsAntiPhase { get; set; }
    public ParamLine Amplitude { get; set; } = new ParamLine();
    public ParamLine Frequency { get; set; } = new ParamLine();

    public Vibrato Decode(SingingTool.Model.Note note)
    {
        var percent = note.VibratoPercentInfo;
        if (percent != null)
        {
            StartPercent = percent.StartPercent;
            EndPercent = percent.EndPercent;
        }
        else if (note.VibratoPercent > 0)
        {
            StartPercent = 1.0f - note.VibratoPercent / 100.0f;
            EndPercent = 1.0f;
        }
        var vibrato = note.Vibrato;
        IsAntiPhase = vibrato.IsAntiPhase;
        Amplitude.Decode(vibrato.AmpLine);
        Frequency.Decode(vibrato.FreqLine);
        return this;
    }

    public Tuple<SingingTool.Model.VibratoPercentInfo,SingingTool.Model.VibratoStyle> Encode()
    {
        var percent = new SingingTool.Model.VibratoPercentInfo(StartPercent, EndPercent);
        var vibrato = new SingingTool.Model.VibratoStyle
        {
            IsAntiPhase = IsAntiPhase
        };
        vibrato.AmpLine.ReplaceParamNodesInPosRange(-1, 100001, 
            Amplitude.Encode(left: -1, right: 100001));
        vibrato.FreqLine.ReplaceParamNodesInPosRange(-1, 100001, 
            Frequency.Encode(left: -1, right: 100001));
        return new Tuple<SingingTool.Model.VibratoPercentInfo, SingingTool.Model.VibratoStyle>(percent, vibrato);
    }
}

public class Params
{
    public ParamLine Pitch { get; set; } = new();
    public ParamLine Volume { get; set; } = new();
    public ParamLine Breath { get; set; } = new();
    public ParamLine Gender { get; set; } = new();
    public ParamLine Strength { get; set; } = new();

    public Params Decode(SingingTool.Model.SingingTrack track)
    {
        if (track.EditedPitchLine != null)
        {
            Pitch.Decode(track.EditedPitchLine, op: x => x > 1050 ? x - 1150 : -100);
        }
        if (track.EditedVolumeLine != null)
        {
            Volume.Decode(track.EditedVolumeLine);
        }
        if (track.EditedBreathLine != null)
        {
            Breath.Decode(track.EditedBreathLine);
        }
        if (track.EditedGenderLine != null)
        {
            Gender.Decode(track.EditedGenderLine);
        }
        if (track.EditedPowerLine != null)
        {
            Strength.Decode(track.EditedPowerLine);
        }
        return this;
    }

    public Hashtable Encode()
    {
        return new Hashtable
        {
            {"Pitch", Pitch.Encode(op: x => x > -100 ? x + 1150 : -100)},
            {"Volume", Volume.Encode()},
            {"Breath", Breath.Encode()},
            {"Gender", Gender.Encode()},
            {"Strength", Strength.Encode()}
        };
    }
}

public class ParamLine
{
    public int TotalPointsCount { get; set; }
    [JsonConverter(typeof(PointListJsonConverter))]
    public List<Tuple<int, int>> PointList { get; set; } = new();

    public ParamLine Decode(
        SingingTool.Model.Line.LineParam line,
        Func<int, int> op = null)
    {
        op ??= x => x;
        TotalPointsCount = line.Length();
        var point = line.Begin;
        for (var i = 0; i < TotalPointsCount && point != null; i++)
        {
            var value = point.Value;
            PointList.Add(new Tuple<int, int>(value.Pos, op(value.Value)));
            point = point.Next;
        }
        return this;
    }

    public SingingTool.Model.Line.LineParam Encode(
        Func<int, int> op = null,
        int left = -192000,
        int right = 1073741823,
        int @default = 0)
    {
        op ??= x => x;
        var line = new SingingTool.Model.Line.LineParam();
        var length = Math.Max(0, Math.Min(TotalPointsCount, PointList.Count));
        PointList.Sort((p1, p2) => p1.Item1 - p2.Item1);
        for (var i = 0; i < length; i++)
        {
            if (PointList[i].Item1 >= left && PointList[i].Item1 <= right)
            {
                line.PushBack(
                    new SingingTool.Model.Line.LineParamNode(PointList[i].Item1, op(PointList[i].Item2)));
            }
        }
        if (length == 0 || line.Begin.Value.Pos > left)
        {
            line.PushFront(new SingingTool.Model.Line.LineParamNode(left, @default));
        }

        if (length == 0 || line.Back.Pos < right)
        {
            line.PushBack(new SingingTool.Model.Line.LineParamNode(right, @default));
        }
        return line;
    }
}
