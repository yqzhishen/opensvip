using System.ComponentModel;

namespace FlutyDeer.SrtPlugin.Options
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
