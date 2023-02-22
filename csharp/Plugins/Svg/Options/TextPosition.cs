using System.ComponentModel;

namespace CrSjimo.SvgPlugin.Options {
    public enum TextPosition {
        [Description("inner")] Inner,
        [Description("upper")] Upper,
        [Description("lower")] Lower,
        [Description("none")] None,
    }
}