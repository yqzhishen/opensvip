using Newtonsoft.Json;

namespace Plugin.Dspx.Model
{
    public class DsNote
    {
        [JsonProperty("pos")]
        public int Position { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("keyNum")]
        public int KeyNumber { get; set; }

        [JsonProperty("lyric")]
        public string Lyric { get; set; }

        [JsonProperty("phonemes")]
        public DsPhonemes Phonemes { get; set; }

        [JsonProperty("vibrato")]
        public DsVibrato Vibrato { get; set; }

        [JsonProperty("extra")]
        public DsExtra Extra { get; set; }

        [JsonProperty("workspace")]
        public DsWorkspace Workspace { get; set; }
    }
}