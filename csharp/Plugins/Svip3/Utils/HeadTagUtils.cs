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
    }
}
