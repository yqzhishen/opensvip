using OpenSvip.Model;
using System;
using System.Collections.Generic;

namespace FlutyDeer.MidiPlugin.Utils
{
    public static class NoteOverlapUtil
    {
        public static bool IsOverlapedItemsExists(List<Note> noteList)
        {
            List<Tuple<int, int>> pairs = new List<Tuple<int, int>>();
            foreach (var note in noteList)
            {
                pairs.Add(new Tuple<int, int>(note.StartPos, note.StartPos + note.Length));
            }
            for (int i = 1; i < pairs.Count; i++)
            {
                if (pairs[i - 1].Item2 > pairs[i].Item1)//前一个音符的结束时间大于后一个音符的开始时间
                {
                    return true;
                }
            }
            return false;
        }
    }
}