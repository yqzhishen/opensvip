using System;
using System.IO;

namespace OpenSvip.Framework
{
    public static class ConstValues
    {
        public const string FrameworkName = "OpenSVIP";

        public const string FrameworkVersion = "1.2.2";

        public static readonly string CommonDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FrameworkName);

        public static readonly string CommonConfigPath = Path.Combine(CommonDataPath, "Config");

        public static readonly string ProjectHomePage = "https://openvpi.github.io/home";

        public static readonly string AuthorHomePage = "https://space.bilibili.com/102844209";

        public static readonly string BundleShareLink = "https://share.weiyun.com/yMDgO6sz";

        public static readonly string PluginMarket = "https://openvpi.github.io/home";
    }
}
