using System;
using System.IO;

namespace OpenSvip.Framework
{
    public static class ConstValues
    {
        public const string FrameworkName = "OpenSvip";

        public const string FrameworkVersion = "1.2.2";

        public static readonly string CommonDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FrameworkName);

        public static readonly string CommonConfigPath = Path.Combine(CommonDataPath, "Config");
    }
}
