using Newtonsoft.Json;
using System.Collections.Generic;

namespace Gjgj.Model
{
    public class GjProjectSetting
    {
        public string No1KeyName { get; set; }
        public string EQAfterMix { get; set; }
    }

    public class GjMasterVolume
    {
        public float Volume { get; set; }
        public float LeftVolume { get; set; }
        public float RightVolume { get; set; }
        public bool Mute { get; set; }
    }

    public class GjAccompanimentsItem
    {
        public string ID { get; set; }
        public string Path { get; set; }
        public int Offset { get; set; }
        public GjMasterVolume MasterVolume { get; set; }
        public string EQProgram { get; set; }
    }

    public class GjBeatItemsItem
    {
        [JsonProperty("ID")]public int NoteID { get; set; }
        public string Lyric { get; set; }
        public string Pinyin { get; set; }
        public int StartTick { get; set; }
        public int Duration { get; set; }
        [JsonProperty("Track")]public int KeyNumber { get; set; }
        public double PreTime { get; set; }
        public double PostTime { get; set; }
        public int Style { get; set; }
    }

    public class GjModifysItem
    {
        [JsonProperty("X")]public double Time { get; set; }
        [JsonProperty("Y")]public double Value { get; set; }
    }

    public class GjOriginsItem
    {
        [JsonProperty("X")]public double Time { get; set; }
        [JsonProperty("Y")]public double Value { get; set; }
    }

    public class GjModifyRangesItem
    {
        [JsonProperty("X")]public double Left { get; set; }
        [JsonProperty("Y")]public double Right { get; set; }
    }

    public class GjTone
    {
        [JsonProperty("Modifys")]public List<GjModifysItem> PitchPointList { get; set; }
        [JsonProperty("Origins")]public List<GjOriginsItem> DefaultPitchPointList { get; set; }
        public List<GjModifyRangesItem> ModifyRanges { get; set; }
    }

    public class GjVolumeMapItem
    {
        public double Time { get; set; }
        [JsonProperty("Volume")]public double Value { get; set; }
    }

    public class GjTracksItem
    {
        [JsonProperty("ID")]public string TrackID { get; set; }
        public string Name { get; set; }
        [JsonProperty("BeatItems")]public List<GjBeatItemsItem> NoteList { get; set; }
        [JsonProperty("Tone")]public GjTone PitchParam { get; set; }
        [JsonProperty("VolumeMap")]public List<GjVolumeMapItem> VolumeParam { get; set; }
        public GjMasterVolume MasterVolume { get; set; }
        public string EQProgram { get; set; }
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
        [JsonProperty("Accompaniments")]public List<GjAccompanimentsItem> Instrumental { get; set; }
        public List<GjTracksItem> Tracks { get; set; }
        public GjTempoMap TempoMap { get; set; }
    }
}