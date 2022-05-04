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
            new MidiEncoder
            {
                Transpose = options.GetValueAsInteger("transpose", 0),
                IsUseCompatibleLyric = options.GetValueAsBoolean("compatibleLyric", false),
                IsRemoveSymbols = options.GetValueAsBoolean("removeSymbols", true),
                LyricEncoding = options.GetValueAsEnum("lyricEncoding", LyricEncodings.UTF8BOM),
                PPQ = options.GetValueAsInteger("ppq", 480)
            }.EncodeMidiFile(project, path);
        }
    }
}