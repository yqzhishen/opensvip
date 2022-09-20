using FlutyDeer.Svip3Plugin.Model;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class HeadTagUtils
    {
        public static string Decode(Xs3Note note)
        {
            if (note.SilLength > 0)
            {
                return "0";
            }
            else if (note.SilLength > 0)
            {
                return "V";
            }
            else
            {
                return null;
            }
        }

        public static int EncodeSilLen(string headTag)
        {
            if (headTag != null)
            {
                return headTag.Equals("0") ? 400 : 0;
            }
            else
            {
                return 0;
            }
        }

        public static int EncodeSpLen(string headTag)
        {
            if (headTag != null)
            {
                return headTag.Equals("V") ? 400 : 0;
            }
            else
            {
                return 0;
            }
        }
    }
}
