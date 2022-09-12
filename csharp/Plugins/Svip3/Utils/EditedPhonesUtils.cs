using OpenSvip.Model;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class EditedPhonesUtils
    {
        public static Phones Decode(Xstudio.Proto.Note note)
        {
            if (note.ConsonantLen > 0)
            {
                return new Phones
                {
                    HeadLengthInSecs = (float)(note.ConsonantLen / 480.0 * 60.0 / TempoListUtils.SongTempoList[0].BPM)
                };
            }
            else
            {
                return null;
            }
        }
    }
}
