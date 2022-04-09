using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CommandLine;
using OpenSvip.Framework;
using OpenSvip.Model;

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
            Project project;
            IProjectConverter inputConverter;
            IProjectConverter outputConverter;
            try
            {
                inputConverter = PluginManager.GetConverter(options.InType);
                outputConverter = PluginManager.GetConverter(options.OutType);
            }
            catch (Exception e)
            {
                HandleError(e, ErrorTypes.Prepare);
                return 2;
            }
            
            try
            {
                project = inputConverter.Load(options.InPath, options.InputOptions);
                
                var warnings = Warnings.GetWarnings();
                if (warnings.Any())
                {
                    System.Console.WriteLine($"来自输入插件 {PluginManager.GetPlugin(options.InType).Name} 的警告信息：");
                    foreach (var warning in warnings)
                    {
                        System.Console.WriteLine(warning);
                    }

                    Warnings.ClearWarnings();
                }
            }
            catch (Exception e)
            {
                HandleError(e, ErrorTypes.Import);
                return 3;
            }

            try
            {
                outputConverter.Save(options.OutPath, project, options.OutputOptions);
                
                var warnings = Warnings.GetWarnings();
                if (warnings.Any())
                {
                    System.Console.WriteLine($"来自输出插件 {PluginManager.GetPlugin(options.OutType).Name} 的警告信息：");
                    foreach (var warning in warnings)
                    {
                        System.Console.WriteLine(warning);
                    }

                    Warnings.ClearWarnings();
                }
            }
            catch (Exception e)
            {
                HandleError(e, ErrorTypes.Export);
                return 3;
            }
            return 0;
        }

        private static void HandleError(Exception exception, ErrorTypes type)
        {
            var ty = typeof(ErrorTypes);
            var situation = ty.GetField(ty.GetEnumName(type)).GetCustomAttribute<DescriptionAttribute>().Description;
#if RELEASE
            System.Console.WriteLine($"{situation}发生错误：" + exception.Message);
#else
            System.Console.WriteLine($"{situation}发生错误。\n{exception.StackTrace}");
#endif
        }

        private static int ManagePlugins(PluginsOptions options)
        {
            if (options.Identifier == null)
            {
                PrintPluginSummary(PluginManager.GetAllPlugins());
            }
            else
            {
                PrintPluginDetails(PluginManager.GetPlugin(options.Identifier));
            }
            return 0;
        }
        
        private static void PrintPluginSummary(Plugin[] plugins)
        {
            System.Console.WriteLine();
            if (!plugins.Any())
            {
                System.Console.WriteLine("当前未安装任何插件。\n");
            }
            var num = 1;
            System.Console.WriteLine("    名称\t版本\t作者\t标识符\t适用格式");
            foreach (var plugin in plugins)
            {
                System.Console.WriteLine($"[{num}] {plugin.Name}\t{plugin.Version}\t{plugin.Author}\t{plugin.Identifier}\t{plugin.Format} (*.{plugin.Suffix})");
                ++num;
            }
            System.Console.WriteLine();
        }

        private static void PrintPluginDetails(Plugin plugin)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("--------------------------------------------------\n");
            System.Console.WriteLine($"插件：{plugin.Name}\t版本：{plugin.Version}\t作者：{plugin.Author}");
            if (plugin.HomePage != null)
            {
                System.Console.WriteLine($"\n主页：{plugin.HomePage}");
            }
            System.Console.WriteLine($"\n此插件适用于 {plugin.Format} (*.{plugin.Suffix})。");
            System.Console.WriteLine($"若要使用此插件，请在转换时指定 \"-i {plugin.Identifier}\" 或 \"-o {plugin.Identifier}\"。");
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
                                System.Console.WriteLine("  可用值如下：");
                                foreach (var choice in option.EnumChoices)
                                {
                                    System.Console.Write($"    {choice.Tag}\t=>\t{choice.Name}");
                                    if (string.IsNullOrWhiteSpace(choice.Label))
                                    {
                                        System.Console.WriteLine();
                                    }
                                    else
                                    {
                                        System.Console.WriteLine($"（{choice.Label}）");
                                    }
                                }
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }
            System.Console.WriteLine("\n--------------------------------------------------\n");
        }
    }
}
