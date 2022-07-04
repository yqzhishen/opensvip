using System.ComponentModel;

namespace FlutyDeer.LyricsPlugin.Options
{
    public enum SplitByOption
    {
        [Description("both")]
        Both,
        [Description("gap")]
        Gap,
        [Description("symbol")]
        Symbol
    }
}
