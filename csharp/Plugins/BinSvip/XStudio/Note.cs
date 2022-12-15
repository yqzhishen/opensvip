namespace XSAppModel.XStudio
{

    public enum NoteHeadTag
    {
        NoTag,
        SilTag,
        SpTag,
    };

    public class NotePhoneInfo
    {
        public NotePhoneInfo() : this(0, 0)
        {
        }

        public NotePhoneInfo(float head, float mid)
        {
            HeadPhoneTimeInSec = head;
            MidPartOverTailPartRatio = mid;
        }

        /* Properties */
        public float HeadPhoneTimeInSec;
        public float MidPartOverTailPartRatio;
    };

    public class Note : IOverlappable
    {
        public Note()
        {
            VibratoPercent = 0;
            startPos = 0;
            widthPos = 480;
            keyIndex = 60;
            headTag = NoteHeadTag.NoTag;

            NotePhoneInfo = null;
            Vibrato = null;
            VibratoPercentInfo = null;
            lyric = "";
            pronouncing = "";
        }

        /* Properties */
        public NotePhoneInfo NotePhoneInfo;

        public int VibratoPercent;
        public VibratoStyle Vibrato;
        public VibratoPercentInfo VibratoPercentInfo;

        /* Members */
        public int startPos;
        public int widthPos;
        public int keyIndex;
        public string lyric;
        public string pronouncing;
        public NoteHeadTag headTag;
    }
}