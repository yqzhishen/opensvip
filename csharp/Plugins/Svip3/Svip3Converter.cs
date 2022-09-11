using OpenSvip.Framework;
using OpenSvip.Model;
using System;
using System.IO;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin.Stream
{
    internal class Svip3Converter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            AppModel model;
            using (var input = File.OpenRead(path))
            {
                model = AppModel.Parser.ParseFrom(input);
            }
            return new Svip3Decoder().Decode(model);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
