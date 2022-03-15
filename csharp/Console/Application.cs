using System.Linq;
using CommandLine;
using OpenSvip.Framework;

namespace OpenSvip.Console
{
    internal static class Application
    {
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ConvertOptions, PluginsOptions>(args)
                .MapResult(
                    (ConvertOptions options) => ConvertFile(options),
                    (PluginsOptions options) => ManagePlugins(options),
                    errors => 1);
        }

        private static int ConvertFile(ConvertOptions options)
        {
            var inputConverter = PluginManager.GetConverter(options.InType);
            var outputConverter = PluginManager.GetConverter(options.OutType);
            outputConverter.Save(
                options.OutPath, 
                inputConverter.Load(
                    options.InPath,
                    options.InputOptions),
                options.OutputOptions);
            return 0;
        }

        private static int ManagePlugins(PluginsOptions options)
        {
            System.Console.WriteLine();
            var plugins = PluginManager.GetAllPlugins();
            if (!plugins.Any())
            {
                System.Console.WriteLine("当前未安装任何插件。\n");
            }
            var num = 1;
            foreach (var plugin in plugins)
            {
                System.Console.WriteLine("--------------------------------------------------\n");
                System.Console.WriteLine($"[{num}]\t{plugin.Name}\t版本：{plugin.Version}\t作者：{plugin.Author}");
                if (plugin.HomePage != null)
                {
                    System.Console.WriteLine($"\n主页：{plugin.HomePage}");
                }
                System.Console.WriteLine($"\n此插件可用于转换 {plugin.Format} (*.{plugin.Suffix})。");
                System.Console.WriteLine($"若要使用此插件，请在转换时指定 \"-i {plugin.Identifier}\" 或 \"-o {plugin.Identifier}\"");
                if (plugin.Descriptions != null)
                {
                    System.Console.WriteLine($"\n描述：\n{plugin.Descriptions}");
                }
                if (plugin.Requirements != null)
                {
                    System.Console.WriteLine($"\n环境要求：\n{plugin.Requirements}");
                }
                string[] opArr = {"输入", "输出"};
                Option[][] optionsArr = {plugin.InputOptions, plugin.OutputOptions};
                for (var i = 0; i < 2; i++)
                {
                    if (!optionsArr[i].Any()) continue;
                    System.Console.WriteLine($"\n本插件可指定以下{opArr[i]}转换选项：");
                    foreach (var option in optionsArr[i])
                    {
                        switch (option.Type)
                        {
                            case "string":
                            case "integer":
                            case "double":
                            case "boolean":
                            case "enum":
                                System.Console.Write($"\n  {option.Name}={option.Type.ToUpper()}    {option.Notes}");
                                if (option.Default != null)
                                {
                                    System.Console.WriteLine($"\t默认值：{option.Default}");
                                }
                                if (option.Type == "enum")
                                {
                                    System.Console.WriteLine("\n  可用值如下：");
                                    foreach (var choice in option.EnumChoices)
                                    {
                                        System.Console.WriteLine($"    {choice.Tag}    =>    {choice.Name}");
                                    }
                                }
                                System.Console.WriteLine();
                                break;
                            default:
                                continue;
                        }
                    }
                }
                ++num;
            }
            return 0;
        }
    }
}
