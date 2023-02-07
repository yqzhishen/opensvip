using Newtonsoft.Json;

namespace Json2DiffSinger.Core.Models
{
    public class PhonemeParamsModel : AbstractParamsModel
    {
        /// <summary>
        /// 歌词文本
        /// </summary>
        [JsonProperty("text")]
        public string LyricText { get; set; } = "";

        /// <summary>
        /// 音素序列
        /// </summary>
        [JsonProperty("ph_seq")]
        public string PhonemeSequence { get; set; } = "";

        /// <summary>
        /// 音符音高序列
        /// </summary>
        [JsonProperty("note_seq")]
        public string NoteSequence { get; set; } = "";

        /// <summary>
        /// 音符时长序列
        /// </summary>
        [JsonProperty("note_dur_seq")]
        public string NoteDurationSequence { get; set; } = "";

        /// <summary>
        /// 转音序列
        /// </summary>
        [JsonProperty("is_slur_seq")]
        public string IsSlurSequence { get; set; } = "";

        /// <summary>
        /// 音素时长序列
        /// </summary>
        [JsonProperty("ph_dur")]
        public string PhonemeDurationSequence { get; set; } = "";

        /// <summary>
        /// F0 步长
        /// </summary>
        [JsonProperty("f0_timestep")]
        public string F0TimeStepSize { get; set; }

        /// <summary>
        /// F0 序列
        /// </summary>
        [JsonProperty("f0_seq")]
        public string F0Sequence { get; set; }

        /// <summary>
        /// 输入模式
        /// </summary>
        [JsonProperty("input_type")]
        public string InputType { get; set; } = "phoneme";
    }
}
