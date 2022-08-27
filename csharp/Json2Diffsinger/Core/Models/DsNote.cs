namespace Json2DiffSinger.Core.Models
{
    /// <summary>
    /// ds 音符
    /// </summary>
    public class DsNote
    {
        /// <summary>
        /// 歌词
        /// </summary>
        public string Lyric { get; set; } = "";

        /// <summary>
        /// 音素参数
        /// </summary>
        public DsPhoneme DsPhoneme { get; set; } = new DsPhoneme();

        /// <summary>
        /// 音高
        /// </summary>
        public string NoteName { get; set; } = "";

        /// <summary>
        /// 时长
        /// </summary>
        public float Duration { get; set; } = 0.0f;

        /// <summary>
        /// 是否为转音
        /// </summary>
        public bool IsSlur
        {
            get => Lyric.Contains("-");
        }
    }

    /// <summary>
    /// 音素参数
    /// </summary>
    public class DsPhoneme
    {
        /// <summary>
        /// 声母音素
        /// </summary>
        public DsPhonemeItem Consonant { get; set; } = new DsPhonemeItem();

        /// <summary>
        /// 韵母音素
        /// </summary>
        public DsPhonemeItem Vowel { get; set; } = new DsPhonemeItem();
    }

    /// <summary>
    /// 单个音素
    /// </summary>
    public class DsPhonemeItem
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Phoneme = "";

        /// <summary>
        /// 时长
        /// </summary>
        public float Duration = 0.0f;

        /// <summary>
        /// 音高
        /// </summary>
        public string NoteName = "";
    }
    /// <summary>
    /// 换气 AP 音素参数
    /// </summary>
    public class AspirationDsPhoneme : DsPhoneme
    {
        /// <summary>
        /// 实例化一个换气 AP 音素
        /// </summary>
        /// <param name="duration">时长</param>
        public AspirationDsPhoneme(float duration)
        {
            Vowel.Phoneme = "AP";
            Vowel.Duration = duration;
            Vowel.NoteName = "rest";
        }
    }

    /// <summary>
    /// 换气 AP 音符
    /// </summary>
    public class AspirationDsNote : DsNote
    {
        /// <summary>
        /// 实例化一个换气 AP 音符
        /// </summary>
        /// <param name="phoneme">音素参数</param>
        public AspirationDsNote(float duration, AspirationDsPhoneme phoneme)
        {
            Lyric = "AP";
            DsPhoneme = phoneme;
            NoteName = "rest";
            Duration = duration;
        }
    }

    /// <summary>
    /// 休止 SP 音素参数
    /// </summary>
    public class RestDsPhoneme : DsPhoneme
    {
        /// <summary>
        /// 实例化一个休止 SP 音素
        /// </summary>
        /// <param name="duration">时长</param>
        public RestDsPhoneme(float duration)
        {
            Vowel.Phoneme = "SP";
            Vowel.Duration = duration;
            Vowel.NoteName = "rest";
        }
    }

    /// <summary>
    /// 休止 SP 音符
    /// </summary>
    public class RestDsNote : DsNote
    {
        /// <summary>
        /// 实例化一个休止 SP 音符
        /// </summary>
        /// <param name="phoneme">音素</param>
        public RestDsNote(float duration, RestDsPhoneme phoneme)
        {
            Lyric = "SP";
            DsPhoneme = phoneme;
            NoteName = "rest";
            Duration = duration;
        }
    }
}
