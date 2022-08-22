using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json2DiffSinger.Core.Models
{
    public class PhonemeParamsModel : AbstractParamsModel
    {
        [JsonProperty("text")]
        public string LyricText { get; set; } = "";

        [JsonProperty("ph_seq")]
        public string PhonemeSequence { get; set; } = "";

        [JsonProperty("note_seq")]
        public string NoteSequence { get; set; } = "";

        [JsonProperty("note_dur_seq")]
        public string NoteDurationSequence { get; set; } = "";

        [JsonProperty("is_slur_seq")]
        public string IsSlurSequence { get; set; } = "";

        [JsonProperty("ph_dur")]
        public string PhonemeDurationSequence { get; set; } = "";

        [JsonProperty("input_type")]
        public string InputType { get; set; } = "phoneme";
    }
}
