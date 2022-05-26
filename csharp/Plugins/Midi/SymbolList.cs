using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlutyDeer.MidiPlugin
{
    public class SymbolList
    {
        public static List<string> SymbolToRemoveList()
        {
            string[] unsupportedSymbolArray = { ",", ".", "?", "!", "，", "。", "？", "！" };
            return unsupportedSymbolArray.ToList();
        }
    }
}
