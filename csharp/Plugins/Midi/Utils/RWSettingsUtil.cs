using FlutyDeer.MidiPlugin.Options;
using Melanchall.DryWetMidi.Core;
using OpenSvip.Framework;

namespace FlutyDeer.MidiPlugin.Utils
{
    public static class RWSettingsUtil
    {
        public static ReadingSettings GetReadingSettings(ConverterOptions options)
        {
            var lyricEncoding = options.GetValueAsEnum("lyricEncoding", LyricEncodingOption.UTF8);
            var errorMidiFilePolicy = options.GetValueAsEnum("errorMidiFilePolicy", ErrorMidiFilePolicyOption.Abort);
            ReadingSettings readingSettings = new ReadingSettings();
            readingSettings.TextEncoding = EncodingUtil.GetEncoding(lyricEncoding);
            if (errorMidiFilePolicy == ErrorMidiFilePolicyOption.Ignore)
            {
                readingSettings.InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid;
                readingSettings.InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore;
                readingSettings.InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits;
                readingSettings.MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Ignore;
                readingSettings.NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore;
                readingSettings.NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore;
                readingSettings.UnexpectedTrackChunksCountPolicy = UnexpectedTrackChunksCountPolicy.Ignore;
                readingSettings.UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByteAndOneDataByte;
                readingSettings.UnknownChunkIdPolicy = UnknownChunkIdPolicy.ReadAsUnknownChunk;
                readingSettings.UnknownFileFormatPolicy = UnknownFileFormatPolicy.Ignore;
            }
            return readingSettings;
        }

        public static WritingSettings GetWritingSettings(ConverterOptions options)
        {
            var lyricEncoding = options.GetValueAsEnum("lyricEncoding", LyricEncodingOption.UTF8);
            return new WritingSettings
            {
                TextEncoding = EncodingUtil.GetEncoding(lyricEncoding)
            };
        }
    }
}
