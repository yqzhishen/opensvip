using OpenSvip.Framework;
using OpenSvip.Model;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Json2DiffSinger.Options;
using Json2DiffSinger.Utils;
using Newtonsoft.Json;

namespace Json2DiffSinger.Stream
{
    internal class DiffSingerConverter : IProjectConverter
    {

        public Project Load(string path, ConverterOptions options)
        {
            throw new NotImplementedException("暂不支持读取 ds 参数文件");
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var split = options.GetValueAsBoolean("split", true);
            options.GetValueAsEnum<DictionaryOption>("dictionary");  // Trigger exception with illegal input
            var dictionary = options.GetValueAsString("dictionary");
            var phonemeMode = options.GetValueAsEnum("phonemeMode", PhonemeModeOption.Auto);
            var pitchMode = options.GetValueAsEnum("pitchMode", PitchModeOption.Auto);
            var seed = options.GetValueAsInteger("seed", -1);
            if (split)
            {
                var segments = project.SplitIntoSegments();
                var series = segments.Select(tuple =>
                {
                    var dsParams = new DiffSingerEncoder
                    {
                        Dictionary = dictionary,
                        PhonemeOption = phonemeMode,
                        PitchModeOption = pitchMode
                    }.Encode(tuple.Item2);
                    dsParams.Offset = tuple.Item1;
                    if (seed >= 0)
                    {
                        dsParams.Seed = seed;
                    }
                    return dsParams;
                }).ToArray();
                var formatted = options.GetValueAsBoolean("formatted", true);
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    writer.Write(JsonConvert.SerializeObject(series, formatted ? Formatting.Indented : Formatting.None));
                }
            }
            else
            {
                project.ResetTimeAxis();
                var diffSingerParams = new DiffSingerEncoder
                {
                    Dictionary = dictionary,
                    PhonemeOption = options.GetValueAsEnum("phonemeMode", PhonemeModeOption.Auto),
                    PitchModeOption = options.GetValueAsEnum("pitchMode", PitchModeOption.Auto)
                }.Encode(project);
                if (seed >= 0)
                {
                    diffSingerParams.Seed = seed;
                }
                var formatted = options.GetValueAsBoolean("formatted", true);
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    writer.Write(JsonConvert.SerializeObject(diffSingerParams, formatted ? Formatting.Indented : Formatting.None));
                }
            }
        }
    }
}
