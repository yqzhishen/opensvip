using Newtonsoft.Json;
using System.Collections.Generic;

namespace Vogen.Model
{
    public class VogenProject
    {
        [JsonProperty("timeSig0")]public string TimeSignature { get; set; }
        [JsonProperty("bpm0")]public float BPM { get; set; }
        [JsonProperty("accomOffset")] public int InstrumentalOffset { get; set; }
        [JsonProperty("utts")]public List<VogTrack> TrackList { get; set; }
    }

    public class VogNote
    {
        [JsonProperty("pitch")]public int KeyNumber { get; set; }
        [JsonProperty("lyric")]public string Lyric { get; set; }
        [JsonProperty("rom")]public string Pronunciation { get; set; }
        [JsonProperty("on")]public int StartPosition { get; set; }
        [JsonProperty("dur")]public int Duration { get; set; }
    }

    public class VogTrack
    {
        [JsonProperty("name")]public string TrackName { get; set; }
        [JsonProperty("singerId")]public string SingerName { get; set; }
        [JsonProperty("romScheme")]public string RomScheme { get; set; }
        [JsonProperty("notes")]public List<VogNote> NoteList { get; set; }
    }

}
