using FlutyDeer.MidiPlugin.Options;
using FlutyDeer.MidiPlugin.Utils;
using Melanchall.DryWetMidi.Core;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;

namespace FlutyDeer.MidiPlugin.Stream
{
    public class MidiConverter : IProjectConverter
    {

        public Project Load(string path, ConverterOptions options)
        {
            MidiFile midiFile;
            try
            {
                midiFile = MidiFile.Read(path, RWSettingsUtil.GetReadingSettings(options));
            }
            catch
            {
                throw new NotImplementedException("此文件可能不是 MIDI 文件，请检查导入格式是否有误。");
            }
            return new MidiDecoder
            {
                IsImportTimeSignatures = options.GetValueAsBoolean("importTimeSignatures", true),
                IsImportLyrics = options.GetValueAsBoolean("importLyrics", true),
                DefaultLyric = options.GetValueAsString("defaultLyric", "a"),
                MultiChannelOption = options.GetValueAsEnum("multiChannel", MultiChannelOption.Split),
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
                //ConstantTempo = options.GetValueAsDouble("constantTempo", 120.0),
                PPQ = options.GetValueAsInteger("ppq", 480),
                PreShift = options.GetValueAsInteger("preShift", 0)
            }.EncodeMidiFile(project);
            midiFile.Write(path, true, settings: RWSettingsUtil.GetWritingSettings(options));
        }
    }
}