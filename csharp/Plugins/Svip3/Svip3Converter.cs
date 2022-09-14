using FlutyDeer.Svip3Plugin.Model;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace FlutyDeer.Svip3Plugin.Stream
{
    internal class Svip3Converter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            var model = Svip3Model.Read(path);
            return new Svip3Decoder().Decode(model);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var model = new Svip3Encoder().Encode(project);
            model.Wirte(path);
        }
    }
}
