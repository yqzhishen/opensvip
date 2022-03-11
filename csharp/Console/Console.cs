using CommandLine;
using OpenSvip.Model;
using OpenSvip.Stream;

namespace OpenSvip.Console;

internal static class Application
{
    public static void Main(string[] args)
    {
        var results = Parser.Default.ParseArguments<Options>(args);
        if (results.Errors.Any())
        {
            Environment.Exit(1);
        }
        var options = results.Value;
        Func<string, Project> read = options.InType switch
        {
            "json" => Json.Load,
            "svip" => Binary.Read,
            _ => throw new ArgumentException("当前仅支持 json 和 svip 格式。")
        };
        Action<string, Project> write = options.OutType switch
        {
            "json" => (s, p) => Json.Dump(s, p, options.Indented),
            "svip" => Binary.Write,
            _ => throw new ArgumentException("当前仅支持 json 和 svip 格式。")
        };
        write(options.OutPath!, read(options.InPath!));
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
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
