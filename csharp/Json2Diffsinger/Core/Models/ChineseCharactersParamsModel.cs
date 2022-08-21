using Newtonsoft.Json;

namespace Json2DiffSinger.Core.Models
{
    public class ChineseCharactersParamsModel
    {
        [JsonProperty("text")]
        public string LyricText { get; set; } = "";

        [JsonProperty("notes")]
        public string NoteSequence { get; set; } = "";

        [JsonProperty("notes_duration")]
        public string NoteDurationSequence { get; set; } = "";

        [JsonProperty("input_type")]
        public string InputType { get; set; } = "word";
    }
}
