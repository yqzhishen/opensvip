using System;
using System.Linq;
using CommandLine;
using OpenSvip.Framework;

namespace OpenSvip.Console
{
    internal static class Application
    {
        public static void Main(string[] args)
        {
            var results = Parser.Default.ParseArguments<ConsoleOptions>(args);
            if (results.Errors.Any())
            {
                Environment.Exit(1);
            }
            var options = results.Value;
            var inputConverter = PluginManager.GetConverter(options.InType);
            var outputConverter = PluginManager.GetConverter(options.OutType);
            outputConverter.Save(options.OutPath, inputConverter.Load(options.InPath));
        }
    }

    internal class ConsoleOptions
    {
        [Option('i', "input-type", Required = true,
            HelpText = "输入文件格式（json 或 svip）", MetaValue = "FORMAT")]
        public string InType { get; set; }

        [Option('o', "output-type", Required = true,
            HelpText = "输出文件格式（json 或 svip）", MetaValue = "FORMAT")]
        public string OutType { get; set; }

        [Value(0, Required = true, HelpText = "源文件路径", MetaValue = "FILE")]
        public string InPath { get; set; }

        [Value(1, Required = true, HelpText = "目标文件路径", MetaValue = "FILE")]
        public string OutPath { get; set; }

        [Option(Default = false, Required = false, HelpText = "是否输出带缩进格式的 JSON 文件")]
        public bool Indented { get; set; }
    }
}
