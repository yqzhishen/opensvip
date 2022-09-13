namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class HeadTagUtils
    {
        public static string Decode(Xstudio.Proto.Note note)
        {
            if (note.HasSilLen)
            {
                return "0";
            }
            else if (note.HasSpLen)
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
            return headTag.Equals("0") ? 400 : 0;
        }

        public static int EncodeSpLen(string headTag)
        {
            return headTag.Equals("V") ? 400 : 0;
        }
    }
}
