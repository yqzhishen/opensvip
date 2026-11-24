using OpenSvip.Framework;
using OpenSvip.Model;


namespace OxygenDioxide.UstPlugin.Stream
{
    public class UstConverter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            return UstDecoder.DecodeFile(path);
        }
        public void Save(string path, Project project, ConverterOptions options)
        {
            var ustEncoder = new UstEncoder
            {
                TrackIndex = options.GetValueAsInteger("TrackIndex", 0),
            };
            ustEncoder.SaveFile(project, path);
        }
    }
}
