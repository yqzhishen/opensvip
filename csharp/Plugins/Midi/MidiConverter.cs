using OpenSvip.Framework;
using OpenSvip.Model;
using Plugin.Midi;

namespace Midi.Stream
{
    internal class MidiConverter : IProjectConverter
    {

        public Project Load(string path, ConverterOptions options)
        {
            return new MidiDecoder().DecodeMidiFile(path);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            new MidiEncoder().EncodeMidiFile(project, path);
        }
    }
}