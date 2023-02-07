using System.ComponentModel;

namespace BinSvip.Stream
{
    public enum BinarySvipVersions
    {
        [Description("auto")]
        Automatic,
        [Description("7.0.0")]
        SVIP7_0_0,
        [Description("6.0.0")]
        SVIP6_0_0,
        [Description("0.0.0")]
        Compatible
    }
}
