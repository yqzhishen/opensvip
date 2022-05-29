using System;
using System.Collections.Generic;
using OpenSvip.Model;

namespace FlutyDeer.MidiPlugin
{
    public class NoteOverlapUtil
    {
        public bool IsOverlapedItemsExists(List<Note> noteList)
        {
            List<Tuple<int, int>> noteStartAndEndTimePairList = new List<Tuple<int, int>>();
            foreach (var note in noteList)
            {
                noteStartAndEndTimePairList.Add(new Tuple<int, int>(note.StartPos, note.StartPos + note.Length));
            }
            for (int i = 1; i < noteStartAndEndTimePairList.Count; i++)
            {
                if (noteStartAndEndTimePairList[i - 1].Item2 > noteStartAndEndTimePairList[i].Item1)//前一个音符的结束时间大于后一个音符的开始时间
                {
                    return true;
                }
            }
            return false;
        }
    }
}