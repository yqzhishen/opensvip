using System.IO;
using OpenSvip.Framework;
using OpenSvip.Model;
using OxygenDioxide.DvPlugin.Model;

namespace OxygenDioxide.DvPlugin.Stream
{
    public class DvConverter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                var dvProject = DvProject.Parse(reader);
                return DvDecoder.DecodeProject(dvProject);
            }
        }
        
        public void Save(string path, Project project, ConverterOptions options)
        {
            throw new System.NotImplementedException();
        }
    }
}
