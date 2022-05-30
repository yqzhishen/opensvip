using Newtonsoft.Json;
using System.Collections.Generic;

namespace FlutyDeer.GjgjPlugin.Model
{
    public class GjProject
    {
        public int gjgjVersion { get; set; }

        public GjProjectSetting ProjectSetting { get; set; }

        [JsonProperty("Accompaniments")]
        public List<GjInstrumentalTrack> InstrumentalTrackList { get; set; }

        [JsonProperty("Tracks")]
        public List<GjSingingTrack> SingingTrackList { get; set; }

        [JsonProperty("MIDITracks")]
        public List<GjMIDITrack> MIDITrackList { get; set; }

        public GjTempoMap TempoMap { get; set; }
    }

    public class GjProjectSetting
    {
        public string No1KeyName { get; set; }

        public string EQAfterMix { get; set; }

        public int ProjectType { get; set; }

        public int Denominator { get; set; }

        public int SynMode { get; set; }
    }

    public class GjTrackVolume
    {
        public float Volume { get; set; }

        public float LeftVolume { get; set; }

        public float RightVolume { get; set; }

        public bool Mute { get; set; }
    }

    public class GjInstrumentalTrack
    {
        [JsonProperty("ID")]
        public string TrackID { get; set; }

        public string Path { get; set; }

        public int Offset { get; set; }

        [JsonProperty("MasterVolume")]
        public GjTrackVolume TrackVolume { get; set; }

        public string EQProgram { get; set; }

        public int SortIndex { get; set; }
    }

    public class GjNote
    {
        [JsonProperty("ID")]
        public int NoteID { get; set; }

        public string Lyric { get; set; }

        public string Pinyin { get; set; }

        public int StartTick { get; set; }

        public int Duration { get; set; }

        [JsonProperty("Track")]
        public int KeyNumber { get; set; }

        [JsonProperty("PreTime")]
        public double PhonePreTime { get; set; }

        [JsonProperty("PostTime")]
        public double PhonePostTime { get; set; }

        public int Style { get; set; }

        public int Velocity { get; set; }
    }

    public class GjPitchParamPoint
    {
        [JsonProperty("X")]
        public double Time { get; set; }

        [JsonProperty("Y")]
        public double Value { get; set; }
    }

    public class GjDefaultPitchParamPoint
    {
        [JsonProperty("X")]
        public double Time { get; set; }

        [JsonProperty("Y")]
        public double Value { get; set; }
    }

    public class GjModifyRange
    {
        [JsonProperty("X")]
        public double Left { get; set; }

        [JsonProperty("Y")]
        public double Right { get; set; }
    }

    public class GjPitchParam
    {
        [JsonProperty("Modifys")]
        public List<GjPitchParamPoint> PitchParamPointList { get; set; }

        [JsonProperty("ModifyRanges")]
        public List<GjModifyRange> ModifyRangeList { get; set; }
    }

    public class GjVolumeParamPoint
    {
        public double Time { get; set; }

        [JsonProperty("Volume")]
        public double Value { get; set; }
    }

    public class GjSingingTrack
    {
        [JsonProperty("ID")]
        public string TrackID { get; set; }

        public int Type { get; set; }

        public string Name { get; set; }

        public int SortIndex { get; set; }

        [JsonProperty("BeatItems")]
        public List<GjNote> NoteList { get; set; }

        [JsonProperty("Tone")]
        public GjPitchParam PitchParam { get; set; }

        [JsonProperty("VolumeMap")]
        public List<GjVolumeParamPoint> VolumeParam { get; set; }

        public GjSingerInfo SingerInfo { get; set; }

        public GjKeyboard Keyboard { get; set; }

        [JsonProperty("MasterVolume")]
        public GjTrackVolume TrackVolume { get; set; }

        public string EQProgram { get; set; }
    }

    public class GjSingerInfo
    {
        [JsonProperty("DisplayName")]
        public string SingerName { get; set; }
    }

    public class GjKeyboard
    {
        public int KeyMode { get; set; }

        public int KeyType { get; set; }
    }

    public class GjTempo
    {
        public int Time { get; set; }

        public int MicrosecondsPerQuarterNote { get; set; }
    }

    public class GjTimeSignature
    {
        public int Time { get; set; }

        public int Numerator { get; set; }

        public int Denominator { get; set; }
    }

    public class GjTempoMap
    {
        public int TicksPerQuarterNote { get; set; }

        [JsonProperty("Tempos")]
        public List<GjTempo> TempoList { get; set; }

        [JsonProperty("TimeSignature")]
        public List<GjTimeSignature> TimeSignatureList { get; set; }
    }

    public class GjMIDITrack
    {

    }
}