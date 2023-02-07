using FlutyDeer.Svip3Plugin.Model;
using FlutyDeer.Svip3Plugin.Stream;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlutyDeer.Svip3Plugin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Write();
            Read();
        }

        private static void Write()
        {
            //var model = new Svip3Model();
            //model.TempoList.Add(new Xs3Tempo
            //{
            //    Tempo = 120
            //});
            //model.TimeSignatureList.Add(new Xs3TimeSignature
            //{
            //    Content = new Xs3TimeSignatureContent
            //    {
            //        Numerator = 4,
            //        Denominator = 4
            //    }
            //});
            //model.TrackList.Add(new Xs3AudioTrack
            //{
            //    Name = "测试音频轨",
            //    Color = "#66CCFF"
            //});
            //model.Write(@"D:\测试\test.svip3");
            string[] paths =
            {
                @"F:\编曲学习\恋人心\恋人心.json",
                @"D:\测试\xs音高参数转换测试.json",
                @"D:\编曲学习\心疼哥哥\歌声合成工程\心疼哥哥（恢复性别）.json",
                @"D:\测试\空工程.json",
                @"F:\编曲学习\惊鹊\歌声合成工程2.0\惊鹊-主.json",
                @"D:\编曲学习\世界这么大还是遇见你\何畅-世界这么大还是遇见你6.0.json"
            };
            Project project;
            using (var stream = new FileStream(
                paths[5],
                FileMode.Open,
                FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                project = JsonConvert.DeserializeObject<Project>(reader.ReadToEnd());
            }
            new Svip3Converter().Save(
                @"D:\编曲学习\世界这么大还是遇见你\何畅-世界这么大还是遇见你6.0.svip3",
                project,
                new ConverterOptions(new Dictionary<string, string>()));
        }

        private static void Read()
        {
            Svip3Model model;
            string[] paths =
            {
                @"D:\编曲学习\恋人心\恋人心.svip3",
                @"D:\测试\音高参数\音高参数.svip3",
                @"D:\测试\心疼哥哥（恢复性别）.svip3",
                @"D:\测试\测试.svip3",
                @"D:\编曲学习\世界这么大还是遇见你\何畅-世界这么大还是遇见你6.0.json",
                @"D:\测试\玻璃做的少女.svip3"
            };
            model = Svip3Model.Read(paths.Last());
            Console.WriteLine(model.Version);
            var audioTrack = model.TrackList.OfType<Xs3AudioTrack>().First();
            var pattern = audioTrack.PatternList.First();
            Console.WriteLine(pattern.AudioFilePath);
            //var singingTrack = model.TrackList.OfType<Xs3SingingTrack>().First();
            //Console.WriteLine(singingTrack.Gain);
            //var pattern = singingTrack.PatternList.First();
            //foreach (var point in pattern.PitchParam)
            //{
            //    Console.Write($"({point.Position},{point.Value})");
            //}
            Console.ReadLine();
        }
    }
}
