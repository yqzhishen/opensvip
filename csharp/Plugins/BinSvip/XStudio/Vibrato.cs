namespace XSAppModel.XStudio
{
    public class VibratoStyle
    {
        public VibratoStyle()
        {
            IsAntiPhase = false;

            ampLine = null;
            freqLine = null;
        }

        /* Properties */
        public bool IsAntiPhase;

        /* Members */
        public LineParam ampLine;
        public LineParam freqLine;
    }

    public class VibratoPercentInfo
    {
        public VibratoPercentInfo() : this(0, 100)
        {
        }

        public VibratoPercentInfo(float start, float end)
        {
            startPercent = start;
            endPercent = end;
        }

        /* Members */
        public float startPercent;
        public float endPercent;
    };
}