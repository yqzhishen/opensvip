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
        public int ID { get; set; }
        public string Lyric { get; set; }
        public string Pinyin { get; set; }
        public int StartTick { get; set; }
        public int Duration { get; set; }
        public int Track { get; set; }
        public double PreTime { get; set; }
        public double PostTime { get; set; }
        public int Style { get; set; }
    }

    public class GjModifysItem
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class GjOriginsItem
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class GjModifyRangesItem
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class GjTone
    {
        public List<GjModifysItem> Modifys { get; set; }
        //public List<OriginsItem> Origins { get; set; }
        public List<GjModifyRangesItem> ModifyRanges { get; set; }
    }

    public class GjVolumeMapItem
    {
        public int Time { get; set; }
        public double Volume { get; set; }
    }

    public class GjTracksItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public List<GjBeatItemsItem> BeatItems { get; set; }
        public GjTone Tone { get; set; }
        public List<GjVolumeMapItem> VolumeMap { get; set; }
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
        public List<GjAccompanimentsItem> Accompaniments { get; set; }
        public List<GjTracksItem> Tracks { get; set; }
        public GjTempoMap TempoMap { get; set; }
    }
}