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
            throw new NotImplementedException();
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var split = options.GetValueAsBoolean("split", true);
            var mode = options.GetValueAsEnum("mode", ModeOption.AutoPhoneme);
            var seed = options.GetValueAsInteger("seed", -1);
            if (split)
            {
                var segments = project.SplitIntoSegments();
                var series = segments.Select(tuple =>
                {
                    var dsParams = new DiffSingerEncoder
                    {
                        InputModeOption = mode
                    }.EncodeParams(tuple.Item2);
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
                var diffSingerParams = new DiffSingerEncoder
                {
                    InputModeOption = options.GetValueAsEnum("mode", ModeOption.AutoPhoneme)
                }.EncodeParams(project);
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
