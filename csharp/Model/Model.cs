using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenSvip.Serialization;

namespace OpenSvip.Model
{
    public class Project
    {
        public string Version { get; set; } = "";

        public List<SongTempo> SongTempoList { get; set; } = new List<SongTempo>();

        public List<TimeSignature> TimeSignatureList { get; set; } = new List<TimeSignature>();

        public List<Track> TrackList { get; set; } = new List<Track>();
    }

    public class SongTempo
    {
        public int Position { get; set; }
        public float BPM { get; set; }
    }

    public class TimeSignature
    {
        public int BarIndex { get; set; }
        public int Numerator { get; set; }
        public int Denominator { get; set; }
    }

    [JsonConverter(typeof(TrackJsonConverter))]
    public class Track
    {
        public readonly string Type;
        public string Title { get; set; } = "";
        public bool Mute { get; set; }
        public bool Solo { get; set; }
        public double Volume { get; set; }
        public double Pan { get; set; }

        protected Track(string type)
        {
            Type = type;
        }
    }

    [JsonConverter(typeof(TrackJsonConverter))]
    public class SingingTrack: Track
    {
        public string AISingerName { get; set; } = "";
        public string ReverbPreset { get; set; } = "";
        public List<Note> NoteList { get; set; } = new List<Note>();
        public Params EditedParams { get; set; } = new Params();

        public SingingTrack() : base("Singing") { }
    }

    [JsonConverter(typeof(TrackJsonConverter))]
    public class InstrumentalTrack: Track
    {
        public string AudioFilePath { get; set; } = "";
        public int Offset { get; set; }
        
        public InstrumentalTrack() : base("Instrumental") { }
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
    }

    public class Phones
    {
        public float HeadLengthInSecs { get; set; } = -1.0f;
        public float MidRatioOverTail { get; set; } = -1.0f;
    }

    public class Vibrato
    {
        public float StartPercent { get; set; }
        public float EndPercent { get; set; }
        public bool IsAntiPhase { get; set; }
        public ParamCurve Amplitude { get; set; } = new ParamCurve();
        public ParamCurve Frequency { get; set; } = new ParamCurve();
    }

    public class Params
    {
        public ParamCurve Pitch { get; set; } = new ParamCurve();
        public ParamCurve Volume { get; set; } = new ParamCurve();
        public ParamCurve Breath { get; set; } = new ParamCurve();
        public ParamCurve Gender { get; set; } = new ParamCurve();
        public ParamCurve Strength { get; set; } = new ParamCurve();
    }

    public class ParamCurve
    {
        public int TotalPointsCount => PointList.Count;
        [JsonConverter(typeof(PointListJsonConverter))]
        public List<Tuple<int, int>> PointList { get; set; } = new List<Tuple<int, int>>();
    }
}
