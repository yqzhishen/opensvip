using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.Y77Plugin
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
