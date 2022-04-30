using Newtonsoft.Json;
using System.Collections.Generic;

namespace Y77.Model
{
    public class Y77Project
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("bars")] public int BarCount { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("notes")] public List<Y77Note> NoteList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("nnote")] public int NoteCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("bpm")] public float BPM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("bbar")] public int TimeSignatureNumerator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("v")] public int Version { get; set; } = 10001;
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("bbeat")] public int TimeSignatureDenominator { get; set; }
    }

    public class Y77Note
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("pbs")] public int PitchBendSensitivity { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("py")] public string Pinyin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("len")] public int Length { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("start")] public int StartPosition { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("lyric")] public string Lyric { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("pitch")] public int KeyNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("pit")] public List<int> PitchParam { get; set; }
    }

}
