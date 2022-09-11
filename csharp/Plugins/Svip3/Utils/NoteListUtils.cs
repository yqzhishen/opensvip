using Google.Protobuf.Collections;
using System.Collections.Generic;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public class NoteListUtils
    {
        public List<OpenSvip.Model.Note> Decode(RepeatedField<SingingPattern> patterns)
        {
            var noteList = new List<OpenSvip.Model.Note>();
            foreach (var pattern in patterns)
            {
                int patternRealPos = pattern.RealPos;
                int patternClipPos = pattern.PlayPos;
            }
            return noteList;
        }

    }
}
