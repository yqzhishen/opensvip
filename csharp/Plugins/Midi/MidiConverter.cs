using FlutyDeer.MidiPlugin.Options;
using FlutyDeer.MidiPlugin.Utils;
using Melanchall.DryWetMidi.Core;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;

namespace FlutyDeer.MidiPlugin.Stream
{
    internal class MidiConverter : IProjectConverter
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
                throw new NotImplementedException("此 MIDI 文件可能不是标准 MIDI 文件、包含错误或损坏，请将“导入有错误或损坏的 MIDI 文件时”设置为“忽略错误”后重试。");
            }
            return new MidiDecoder
            {
                IsImportTimeSignatures = options.GetValueAsBoolean("importTimeSignatures", true),
                IsImportLyrics = options.GetValueAsBoolean("importLyrics", true),
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
                IsUseLegacyPinyin = options.GetValueAsBoolean("isUseLegacyPinyin", false),
                IsRemoveSymbols = options.GetValueAsBoolean("removeSymbols", true),
                //IsConstantTempo = options.GetValueAsBoolean("constantTempo", false),
                PPQ = options.GetValueAsInteger("ppq", 480),
                PreShift = options.GetValueAsInteger("preShift", 0)
            }.EncodeMidiFile(project);
            midiFile.Write(path, true, settings: RWSettingsUtil.GetWritingSettings(options));
        }
    }
}