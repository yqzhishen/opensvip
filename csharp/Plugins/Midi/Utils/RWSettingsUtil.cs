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
            ReadingSettings readingSettings = new ReadingSettings
            {
                TextEncoding = EncodingUtil.GetEncoding(lyricEncoding),
                InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid,
                InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
                InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits,
                MissedEndOfTrackPolicy = MissedEndOfTrackPolicy.Ignore,
                NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore,
                NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
                UnexpectedTrackChunksCountPolicy = UnexpectedTrackChunksCountPolicy.Ignore,
                UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByteAndOneDataByte,
                UnknownChunkIdPolicy = UnknownChunkIdPolicy.ReadAsUnknownChunk,
                UnknownFileFormatPolicy = UnknownFileFormatPolicy.Ignore
            };
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
