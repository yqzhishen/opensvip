using OpenSvip.Framework;
using OpenSvip.Model;
using FlutyDeer.MidiPlugin.Options;

namespace FlutyDeer.MidiPlugin.Stream
{
    internal class MidiConverter : IProjectConverter
    {

        public Project Load(string path, ConverterOptions options)
        {
            return new MidiDecoder
            {
                IsImportTimeSignatures = options.GetValueAsBoolean("importTimeSignatures", true),
                IsImportLyrics = options.GetValueAsBoolean("importLyrics", false),
                LyricEncoding = options.GetValueAsEnum("lyricEncoding", LyricEncodingOption.UTF8BOM),
                MultiChannelOption = options.GetValueAsEnum("multiChannel", MultiChannelOption.First),
                Channels = options.GetValueAsString("channels", "1"),
                ErrorMidiFilePolicy = options.GetValueAsEnum("errorMidiFilePolicy", ErrorMidiFilePolicyOption.Abort)
            }.DecodeMidiFile(path);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            new MidiEncoder
            {
                IsExportLyrics = options.GetValueAsBoolean("exportLyrics", true),
                Transpose = options.GetValueAsInteger("transpose", 0),
                IsUseCompatibleLyric = options.GetValueAsBoolean("compatibleLyric", false),
                IsRemoveSymbols = options.GetValueAsBoolean("removeSymbols", true),
                LyricEncoding = options.GetValueAsEnum("lyricEncoding", LyricEncodingOption.UTF8BOM),
                PPQ = options.GetValueAsInteger("ppq", 480),
                PreShift = options.GetValueAsInteger("preShift", 0)
            }.EncodeMidiFile(project, path);
        }
    }
}