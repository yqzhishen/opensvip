using CommandLine;
using OpenSvip.Framework;
using System;
using System.Collections.Generic;

namespace Json2DiffSinger.Console
{
    /// <summary>
    /// 命令行版 Json2DiffSinger 的参数
    /// </summary>
    [Verb("convert", true, HelpText = "进行格式转换。")]
    public class Arguments
    {
        /// <summary>
        /// 输入文件路径
        /// </summary>
        [Value(0, Required = true, HelpText = "输入文件路径", MetaValue = "FILE")]
        public string InPath { get; set; }

        /// <summary>
        /// 输出文件路径
        /// </summary>
        [Value(1, Required = true, HelpText = "输出文件路径", MetaValue = "FILE")]
        public string OutPath { get; set; }

        /// <summary>
        /// 输出选项
        /// </summary>
        [Option("output-options", Separator = ';', Required = false,
            HelpText = "（可选）输出选项，格式为 [选项名]=[选项值]。多个选项之间以 \";\" 分隔。若想了解程序所支持的选项，请查看“使用说明.docx”。")]
        public IEnumerable<string> _outputOptions { private get; set; } = Array.Empty<string>();

        /// <summary>
        /// 转换选项
        /// </summary>
        public ConverterOptions OutputOptions => new ConverterOptions(OptionsToDictionary(_outputOptions));

        private static Dictionary<string, string> OptionsToDictionary(IEnumerable<string> options)
        {
            var dict = new Dictionary<string, string>();
            foreach (var option in options)
            {
                var kv = option.Split(new[] { '=' }, 2);
                if (kv.Length < 2)
                {
                    throw new ArgumentException("给定的转换选项格式不合法：应为 [选项名]=[选项值]。");
                }
                dict[kv[0]] = kv[1];
            }
            return dict;
        }
    }
}
