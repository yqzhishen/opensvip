#define IMPORT

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AceStdio.Model;
using AceStdio.Resources;
using AceStdio.Stream;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;


namespace AceStdio.Test
{
    public static class Test
    {
        public static void Main(string[] args)
        {
#if BUILD
            var aceProject = new AceProject();
            aceProject.Content.TempoList.Add(new AceTempo
            {
                BPM = 120,
                Position = 0
            });
            aceProject.Content.ColorIndex = 10;
            aceProject.Content.Duration = 19200;
            aceProject.Content.TrackList.Add(new AceVocalTrack
            {
                Color = ColorPool.GetColor(aceProject.Content.ColorIndex++),
                Singer = "荼鸢",
                PatternList = new List<AceVocalPattern>
                {
                    new AceVocalPattern
                    {
                        Duration = 19200,
                        ClipDuration = 19200
                    }
                }
            });
            using (var stream = new FileStream(@"C:\Users\YQ之神\ACE_Studio\project\231e954e732156b9b8a8bab7edd90cdf\颜色测试.acep", FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                var jsonString = JsonConvert.SerializeObject(aceProject);
                writer.Write(jsonString);
            }
#endif
#if IMPORT
            string[] src =
            {
                @"E:\YQ数据空间\YQ实验室\实验室：AceStudio\黏黏黏黏\黏黏黏黏.acep",
                @"C:\Users\YQ之神\Desktop\test space\groups.acep",
                @"C:\Users\YQ之神\Desktop\test.acep",
                @"C:\Users\YQ之神\Desktop\黏黏黏黏\黏黏黏黏.acep",
                @"E:\YQ数据空间\YQ实验室\实验室：AceStudio\囍\囍_冻结参数.acep"
            };
            string[] dst =
            {
                @"C:\Users\YQ之神\Desktop\黏黏黏黏.json",
                @"C:\Users\YQ之神\Desktop\test space\groups.json",
                @"C:\Users\YQ之神\Desktop\test.json",
                @"C:\Users\YQ之神\Desktop\黏黏黏黏.json",
                @"C:\Users\YQ之神\Desktop\囍.json"
            };
            const int index = 4;
            var project = new AceConverter().Load(
                src[index],
                new ConverterOptions(new Dictionary<string, string>
                {
                    /*
                    {"breathNormalization", "zscore, 0, 5.0, 0.15, 0"},
                    {"tensionNormalization", "zscore, 0, 10, 0.6, 0.2"},
                    {"energyNormalization", "zscore, 0, 10, 0.6, 0.2"}
                */
                }));
            using (var stream = new FileStream(dst[index], FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(JsonConvert.SerializeObject(project));
            }
#endif
#if EXPORT
            string[] src =
            {
                @"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\MOM\MOM.json",
                @"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\黏黏黏黏\黏黏黏黏.json",
                @"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\少年弦\少年弦.json",
                @"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\囍（官方示例）\囍（片段）.json",
                @"C:\Users\YQ之神\Desktop\测试参数值域-svip.json",
                @"E:\YQ数据空间\YQ实验室\实验室：AceStudio\囍（片段）\囍（念白片段）.json",
            };
            string[] dst =
            {
                @"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\MOM\MOM.acep",
                @"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\黏黏黏黏\黏黏黏黏.acep",
                @"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\少年弦\少年弦.acep",
                @"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\囍（官方示例）\囍（片段）.acep",
                @"C:\Users\YQ之神\Desktop\测试参数值域.acep",
                @"E:\YQ数据空间\YQ实验室\实验室：AceStudio\囍（片段）\囍（念白片段）.acep"
            };
            const int index = 5;
            using (var stream = new FileStream(src[index], FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                var project = JsonConvert.DeserializeObject<Project>(reader.ReadToEnd());
                new AceConverter().Save(dst[index],
                    project,
                    new ConverterOptions(new Dictionary<string, string>{{"lagCompensation", "30"}, {"breath", "0"}, {"mapStrengthTo", "energy"}}));
            }
#endif
        }
    }
}
