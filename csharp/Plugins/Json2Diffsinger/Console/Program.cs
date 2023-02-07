using CommandLine;
using Json2DiffSinger.Stream;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace Json2DiffSinger.Console
{
    internal partial class Program
    {

        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Arguments>(args)
                .MapResult(
                    (Arguments options) => ConvertFile(options),
                    errors => 1);
        }

        private static int ConvertFile(Arguments options)
        {

            var stream = new FileStream(
                options.InPath,
                FileMode.Open,
                FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var project = JsonConvert.DeserializeObject<Project>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            new DiffSingerConverter().Save(options.OutPath, project, options.OutputOptions);
            return 0;
        }
        private static void HandleError(Exception exception, ErrorTypes type)
        {
            var ty = typeof(ErrorTypes);
            var situation = ty.GetField(ty.GetEnumName(type)).GetCustomAttribute<DescriptionAttribute>().Description;
            System.Console.WriteLine($"{situation}发生错误。{exception.Message}\n{exception.StackTrace}");
        }
    }
}
