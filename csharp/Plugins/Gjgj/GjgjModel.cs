using Newtonsoft.Json;
using System.Collections.Generic;

namespace Gjgj.Model
{
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

    public class GjInstrumentalTracksItem
    {
        [JsonProperty("ID")]public string TrackID { get; set; }
        public string Path { get; set; }
        public int Offset { get; set; }
        [JsonProperty("MasterVolume")]public GjTrackVolume TrackVolume { get; set; }
        public string EQProgram { get; set; }
        public int SortIndex { get; set; }
    }

    public class GjNoteListItem
    {
        [JsonProperty("ID")]public int NoteID { get; set; }
        public string Lyric { get; set; }
        public string Pinyin { get; set; }
        public int StartTick { get; set; }
        public int Duration { get; set; }
        [JsonProperty("Track")]public int KeyNumber { get; set; }
        [JsonProperty("PreTime")]public double PhonePreTime { get; set; }
        [JsonProperty("PostTime")]public double PhonePostTime { get; set; }
        public int Style { get; set; }
        public int Velocity { get; set; }
    }

    public class GjPitchPointListItem
    {
        [JsonProperty("X")]public double Time { get; set; }
        [JsonProperty("Y")]public double Value { get; set; }
    }

    public class GjDefaultPitchPointListItem
    {
        [JsonProperty("X")]public double Time { get; set; }
        [JsonProperty("Y")]public double Value { get; set; }
    }

    public class GjModifyRangesItem
    {
        [JsonProperty("X")]public double Left { get; set; }
        [JsonProperty("Y")]public double Right { get; set; }
    }

    public class GjPitchParam
    {
        [JsonProperty("Modifys")]public List<GjPitchPointListItem> PitchPointList { get; set; }
        [JsonProperty("Origins")]public List<GjDefaultPitchPointListItem> DefaultPitchPointList { get; set; }
        public List<GjModifyRangesItem> ModifyRanges { get; set; }
    }

    public class GjVolumeParamItem
    {
        public double Time { get; set; }
        [JsonProperty("Volume")]public double Value { get; set; }
    }

    public class GjSingingTracksItem
    {
        [JsonProperty("ID")]public string TrackID { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public int SortIndex { get; set; }
        [JsonProperty("BeatItems")]public List<GjNoteListItem> NoteList { get; set; }
        [JsonProperty("Tone")]public GjPitchParam PitchParam { get; set; }
        [JsonProperty("VolumeMap")]public List<GjVolumeParamItem> VolumeParam { get; set; }
        public GjKeyboard Keyboard { get; set; }
        [JsonProperty("MasterVolume")]public GjTrackVolume TrackVolume { get; set; }
        public string EQProgram { get; set; }
    }

    public class GjKeyboard
    {
        public int KeyMode { get; set; }
        public int KeyType { get; set; }
    }

    public class GjTemposItem
    {
        public int Time { get; set; }
        public int MicrosecondsPerQuarterNote { get; set; }
    }

    public class GjTimeSignatureItem
    {
        public int Time { get; set; }
        public int Numerator { get; set; }
        public int Denominator { get; set; }
    }

    public class GjTempoMap
    {
        public int TicksPerQuarterNote { get; set; }
        public List<GjTemposItem> Tempos { get; set; }
        public List<GjTimeSignatureItem> TimeSignature { get; set; }
    }

    public class GjProject
    {
        public int gjgjVersion { get; set; }
        public GjProjectSetting ProjectSetting { get; set; }
        [JsonProperty("Accompaniments")]public List<GjInstrumentalTracksItem> InstrumentalTracks { get; set; }
        [JsonProperty("Tracks")]public List<GjSingingTracksItem> SingingTracks { get; set; }
        public List<GjMIDITracks> MIDITracks { get; set; }
        public GjTempoMap TempoMap { get; set; }
    }

    public class GjMIDITracks
    {

    }
}