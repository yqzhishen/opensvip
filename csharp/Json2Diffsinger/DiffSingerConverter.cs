using OpenSvip.Framework;
using OpenSvip.Model;
using System;
using System.IO;
using System.Text;

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
            var diffSingerParams = new DiffSingerEncoder
            {
                InputModeOption = options.GetValueAsEnum("mode", Options.ModeOption.AutoPhoneme),
                IsFormatted = options.GetValueAsBoolean("formatted", true)
            }.EncodeParams(project);
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(diffSingerParams);
            }
        }

    }
}
