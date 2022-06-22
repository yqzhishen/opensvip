using OpenSvip.Framework;
using OpenSvip.Model;
using FlutyDeer.MidiPlugin.Options;
using Melanchall.DryWetMidi.Core;
using FlutyDeer.MidiPlugin.Utils;

namespace FlutyDeer.MidiPlugin.Stream
{
    internal class MidiConverter : IProjectConverter
    {

        public Project Load(string path, ConverterOptions options)
        {
            var midiFile = MidiFile.Read(path, RWSettingsUtil.GetReadingSettings(options));
            return new MidiDecoder
            {
                IsImportTimeSignatures = options.GetValueAsBoolean("importTimeSignatures", true),
                IsImportLyrics = options.GetValueAsBoolean("importLyrics", false),
                MultiChannelOption = options.GetValueAsEnum("multiChannel", MultiChannelOption.First),
                Channels = options.GetValueAsString("channels", "1")
            }.DecodeMidiFile(midiFile);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var midiFile = new MidiEncoder
            {
                IsExportLyrics = options.GetValueAsBoolean("exportLyrics", true),
                Transpose = options.GetValueAsInteger("transpose", 0),
                IsUseCompatibleLyric = options.GetValueAsBoolean("compatibleLyric", false),
                IsRemoveSymbols = options.GetValueAsBoolean("removeSymbols", true),
                PPQ = options.GetValueAsInteger("ppq", 480),
                PreShift = options.GetValueAsInteger("preShift", 0)
            }.EncodeMidiFile(project);
            midiFile.Write(path, true, settings: RWSettingsUtil.GetWritingSettings(options));
        }
    }
}