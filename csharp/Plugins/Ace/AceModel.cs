using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AceStdio.Stream;
using Newtonsoft.Json;

namespace AceStdio.Model
{
    public class AceProject
    {
        [JsonProperty("compressMethod")] public string CompressMethod { get; set; }

        [JsonProperty("content")]
        // [JsonConverter(typeof(CryptoJsonConverter))]
        public AceContent Content { get; set; } = new AceContent();

        [JsonProperty("debugInfo")] public AceDebug DebugInfo { get; set; } = new AceDebug();

        [JsonProperty("salt", NullValueHandling = NullValueHandling.Ignore)]
        public string Salt { get; set; }

        // [JsonProperty("version")] public int Version { get; set; } = 1;
        [JsonProperty("version")] public int Version { get; set; } = 1000;
    }

    public class AceContent
    {
        [JsonProperty("beatsPerBar")] public int BeatsPerBar { get; set; } = 4;

        [JsonProperty("colorIndex")] public int ColorIndex { get; set; }

        [JsonProperty("duration")] public int Duration { get; set; }

        [JsonProperty("master")] public AceMaster Master { get; set; } = new AceMaster();

        [JsonProperty("pianoCells")] public int PianoCells { get; set; }

        [JsonProperty("tempos")] public List<AceTempo> TempoList { get; set; } = new List<AceTempo>();

        [JsonProperty("trackCells")] public int TrackCells { get; set; }

        [JsonProperty("trackControlPanelW")] public string TrackControlPanelW { get; set; }

        [JsonProperty("tracks")]
        [JsonConverter(typeof(TracksJsonConverter))]
        public List<AceTrack> TrackList { get; set; } = new List<AceTrack>();

        [JsonProperty("version")] public int Version { get; set; } = 5;
    }

    public class AceMaster
    {
        [JsonProperty("gain")] public double Gain { get; set; }
    }

    public class AceTempo
    {
        [JsonProperty("bpm")] public double BPM { get; set; }

        [JsonProperty("isLerp")] public bool IsLerp { get; set; }

        [JsonProperty("position")] public int Position { get; set; }
    }

    public abstract class AceTrack
    {
        [JsonProperty("type")]
        [JsonConverter(typeof(EnumJsonConverter<AceTrackType>))]
        public abstract AceTrackType Type { get; }

        [JsonProperty("channel")] public int Channel { get; set; }

        [JsonProperty("name")] public string Name { get; set; } = "";

        [JsonProperty("color")] public string Color { get; set; } = "#91bcdc";

        [JsonProperty("gain")] public double Gain { get; set; }

        [JsonProperty("language")] public string Language { get; set; }

        [JsonProperty("pan")] public double Pan { get; set; }

        [JsonProperty("listen")] public bool Listen { get; set; }

        [JsonProperty("mute")] public bool Mute { get; set; }

        [JsonProperty("solo")] public bool Solo { get; set; }

        [JsonProperty("record")] public bool Record { get; set; }

        public abstract int Length();
    }

    public class AceVocalTrack : AceTrack
    {
        public override AceTrackType Type => AceTrackType.Vocal;

        // [JsonProperty("singer")]
        // [JsonConverter(typeof(SingerJsonConverter))]
        // public string Singer { get; set; }

        [JsonProperty("singer")] public AceSinger Singer { get; set; }

        [JsonProperty("patterns")] public List<AceVocalPattern> PatternList { get; set; } = new List<AceVocalPattern>();

        public override int Length()
        {
            var lastPattern = PatternList.LastOrDefault();
            if (lastPattern == null)
            {
                return 0;
            }

            return lastPattern.Position + lastPattern.ClipDuration - lastPattern.ClipPosition;
        }
    }

    public class AceSinger
    {
        [JsonProperty("composition")]
        public List<AceComposition> Composition { get; set; } = new List<AceComposition>();

        [JsonProperty("head")] public int Head { get; set; }

        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("router")] public int Router { get; set; }

        [JsonProperty("state")] public string State { get; set; }
    }

    public class AceComposition
    {
        [JsonProperty("code")] public int Code { get; set; }

        [JsonProperty("lock")] public bool Lock { get; set; }

        [JsonProperty("style")] public int Style { get; set; }

        [JsonProperty("timbre")] public int Timbre { get; set; }
    }

    public class AceAudioTrack : AceTrack
    {
        public override AceTrackType Type => AceTrackType.Audio;

        [JsonProperty("patterns")] public List<AceAudioPattern> PatternList { get; set; } = new List<AceAudioPattern>();

        public override int Length()
        {
            var lastPattern = PatternList.LastOrDefault();
            if (lastPattern == null)
            {
                return 0;
            }

            return lastPattern.Position + lastPattern.ClipDuration - lastPattern.ClipPosition;
        }
    }

    public enum AceTrackType
    {
        [Description("sing")] Vocal,
        [Description("audio")] Audio
    }

    public class AcePattern
    {
        [JsonProperty("name")] public string Name { get; set; } = "";

        [JsonProperty("pos")] public int Position { get; set; }

        [JsonProperty("dur")] public int Duration { get; set; }

        [JsonProperty("clipPos")] public int ClipPosition { get; set; }

        [JsonProperty("clipDur")] public int ClipDuration { get; set; }
    }

    public class AceVocalPattern : AcePattern
    {
        // [JsonProperty("language")]
        // public AceLyricsLanguage Language { get; set; } = AceLyricsLanguage.Chinese;

        // [JsonProperty("extendLyrics")] public string ExtendLyrics { get; set; } = "";

        [JsonProperty("notes")] public List<AceNote> NoteList { get; set; } = new List<AceNote>();

        [JsonProperty("parameters")] public AceParams Params { get; set; } = new AceParams();
    }

    [JsonConverter(typeof(EnumJsonConverter<AceLyricsLanguage>))]
    public enum AceLyricsLanguage
    {
        [Description("CHN")] Chinese,
        [Description("JPN")] Japanese,
        [Description("ENG")] English
    }

    public class AceAudioPattern : AcePattern
    {
        [JsonProperty("path")] public string AudioFilePath { get; set; }

        [JsonProperty("gain")] public int Gain { get; set; }

        [JsonProperty("analysedBeat")] public AceAnalysedBeat AnalysedBeat { get; set; }
    }

    public class AceAnalysedBeat
    {
        [JsonProperty("beatTimes")] public List<object> BeatTimes = new List<object>();

        [JsonProperty("length")] public int Length = 0;

        [JsonProperty("offset")] public int Offset = 0;

        [JsonProperty("scales")] public List<object> Scales = new List<object>();
    }

    public class AceNote
    {
        [JsonProperty("pos")] public int Position { get; set; }

        [JsonProperty("dur")] public int Duration { get; set; }

        [JsonProperty("pitch")] public int Pitch { get; set; }

        [JsonProperty("language")] public AceLyricsLanguage Language { get; set; } = AceLyricsLanguage.Chinese;

        [JsonProperty("lyric")] public string Lyrics { get; set; }

        // [JsonProperty("pronunciation")] public string Pronunciation { get; set; }

        [JsonProperty("syllable")] public string Syllable { get; set; }

        [JsonProperty("newLine")] public bool NewLine { get; set; }

        // [JsonProperty("consonantLen", NullValueHandling = NullValueHandling.Ignore)]
        // public int? ConsonantLength { get; set; }

        [JsonProperty("headConsonants")] public List<int> HeadConsonants { get; set; } = new List<int>();

        [JsonProperty("tailConsonants")] public List<int> TailConsonants { get; set; } = new List<int>();

        [JsonProperty("brLen")] public int BreathLength { get; set; }

        [JsonProperty("vibrato")] public AceVibrato Vibrato { get; set; } = new AceVibrato();
    }

    public class AceVibrato
    {
        [JsonProperty("start")] public double Start { get; set; } // maybe int?

        [JsonProperty("amplitude")] public double Amplitude { get; set; }

        [JsonProperty("frequency")] public double Frequency { get; set; }

        [JsonProperty("attackLen")] public double AttackLength { get; set; }

        [JsonProperty("releaseLen")] public double ReleaseLength { get; set; }

        [JsonProperty("releaseVol")] public double ReleaseVolume { get; set; }
    }

    public class AceParams
    {
        [JsonProperty("pitchDelta")] public List<AceParamCurve> PitchDelta { get; set; } = new List<AceParamCurve>();

        [JsonProperty("energy")] public List<AceParamCurve> Energy { get; set; } = new List<AceParamCurve>();

        [JsonProperty("breathiness")] public List<AceParamCurve> Breath { get; set; } = new List<AceParamCurve>();

        [JsonProperty("tension")] public List<AceParamCurve> Tension { get; set; } = new List<AceParamCurve>();

        [JsonProperty("falsetto")] public List<AceParamCurve> Falsetto { get; set; } = new List<AceParamCurve>();

        [JsonProperty("gender")] public List<AceParamCurve> Gender { get; set; } = new List<AceParamCurve>();

        [JsonProperty("realEnergy")] public List<AceParamCurve> RealEnergy { get; set; } = new List<AceParamCurve>();

        [JsonProperty("realBreathiness")]
        public List<AceParamCurve> RealBreath { get; set; } = new List<AceParamCurve>();

        [JsonProperty("realTension")] public List<AceParamCurve> RealTension { get; set; } = new List<AceParamCurve>();

        [JsonProperty("realFalsetto")]
        public List<AceParamCurve> RealFalsetto { get; set; } = new List<AceParamCurve>();

        [JsonProperty("vuv")] public List<object> Vuv { get; set; } = new List<object>();
    }

    public class AceParamCurve
    {
        [JsonProperty("type")] public string Type { get; set; } = "data";

        [JsonProperty("offset")] public int Offset { get; set; }

        [JsonProperty("values")]
        [JsonConverter(typeof(ParamsJsonConverter))]
        public List<double> Values { get; set; } = new List<double>();
    }

    public class AceDebug
    {
        [JsonProperty("os")] public string OS { get; set; } = "windows";

        [JsonProperty("platform")] public string Platform { get; set; } = "pc";

        [JsonProperty("version")] public string Version { get; set; } = "10";
    }
}