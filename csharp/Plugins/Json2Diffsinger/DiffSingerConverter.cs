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
            var splitLength = options.GetValueAsDouble("split", 5.0);
            var minInterval = options.GetValueAsInteger("minInterval", 400);
            options.GetValueAsEnum<DictionaryOption>("dictionary");  // Trigger exception with illegal input
            var dictionary = options.GetValueAsString("dictionary");  // Get the actual value
            var phonemeMode = options.GetValueAsEnum("phonemeMode", PhonemeModeOption.Manual);
            var pitchMode = options.GetValueAsEnum("pitchMode", PitchModeOption.Manual);
            var isExportGender = options.GetValueAsBoolean("gender", true);
            var seed = options.GetValueAsInteger("seed", -1);
            if (splitLength >= 0)
            {
                var segments = project.SplitIntoSegments(minInterval, (int)(splitLength * 1000));
                var series = segments.Select(tuple =>
                {
                    var dsParams = new DiffSingerEncoder
                    {
                        Dictionary = dictionary,
                        PhonemeOption = phonemeMode,
                        PitchModeOption = pitchMode,
                        IsExportGender = isExportGender,
                    }.Encode(tuple.Item2, tuple.Item3);
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
                    PhonemeOption = options.GetValueAsEnum("phonemeMode", PhonemeModeOption.Manual),
                    PitchModeOption = options.GetValueAsEnum("pitchMode", PitchModeOption.Manual),
                    IsExportGender = isExportGender
                }.Encode(project, 0.5f);
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
