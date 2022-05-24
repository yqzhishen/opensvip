using System;
using System.IO;

namespace OpenSvip.Framework
{
    public static class ConstValues
    {
        public const string FrameworkName = "OpenSVIP";

        public const string FrameworkVersion = "1.2.3";

        public static readonly string CommonDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FrameworkName);

        public static readonly string CommonConfigPath = Path.Combine(CommonDataPath, "Config");

        public const string ProjectHomePage = "https://openvpi.github.io/home";

        public const string AuthorHomePage = "https://space.bilibili.com/102844209";

        public const string BundleShareLink = "https://share.weiyun.com/yMDgO6sz";

        public const string PluginMarket = "https://openvpi.github.io/home/market/summary.html";

        public const string FeedbackQQGroup = "687772360";
    }
}
