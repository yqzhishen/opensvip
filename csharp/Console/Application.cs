using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Microsoft.Win32;
using OpenSvip.Adapter;
using OpenSvip.Stream;

namespace OpenSvip.Console
{
    internal static class Application
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += SingingToolResolveEventHandler;
            var results = Parser.Default.ParseArguments<Options>(args);
            if (results.Errors.Any())
            {
                Environment.Exit(1);
            }
            var options = results.Value;
            IProjectConverter inputConverter, outputConverter;
            switch (options.InType)
            {
                case "json":
                    inputConverter = new JsonSvipConverter();
                    break;
                case "svip":
                    inputConverter = new BinarySvipConverter();
                    break;
                default:
                    throw new ArgumentException("当前仅支持 json 和 svip 格式。");
            }
            switch (options.OutType)
            {
                case "json":
                    outputConverter = new JsonSvipConverter(options.Indented);
                    break;
                case "svip":
                    outputConverter = new BinarySvipConverter();
                    break;
                default:
                    throw new ArgumentException("当前仅支持 json 和 svip 格式。");
            }
            outputConverter.Save(options.OutPath, inputConverter.Load(options.InPath));
        }

        private static string FindLibrary()
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\svipfile\shell\open\command");
            if (key == null)
            {
                throw new FileNotFoundException("未检测到已安装的 X Studio · 歌手软件。");
            }
            var value = key.GetValue("").ToString().Split('"')[1];
            return value.Substring(0, value.Length - 18);
        }

        private static Assembly SingingToolResolveEventHandler(object sender, ResolveEventArgs args)
        {
            var path = FindLibrary();
            var filename = args.Name.Split(',')[0];
            return Assembly.LoadFile($@"{path}\{filename}.dll");
        }
    }

    internal class Options
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
