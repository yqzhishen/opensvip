using FlutyDeer.LyricsPlugin.Options;
using FlutyDeer.LyricsPlugin.Utils;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;

namespace FlutyDeer.LyricsPlugin.Stream
{
    public class LyricsConverter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            throw new NotImplementedException("不支持将本插件作为输入端。");
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var lrcFile = new LyricsEncoder {
                Artist = options.GetValueAsString("artist"),
                Title = options.GetValueAsString("title"),
                Album = options.GetValueAsString("album"),
                By = options.GetValueAsString("by"),
                Offset = options.GetValueAsInteger("offset"),
                OffsetPolicyOption = options.GetValueAsEnum("offsetPolicy", OffsetPolicyOption.Timeline),
                SplitByOption = options.GetValueAsEnum("splitBy", SplitByOption.Both),
                lyricsText = options.GetValueAsString("lyricsText"),
                autoInsertBlankLine = options.GetValueAsInteger("autoInsertBlankLine", 4)
            }.EncodeProject(project);
            var lyricEncoding = options.GetValueAsEnum("encoding", LyricEncodingOption.UTF8);
            var writingSettings = new WritingSettings
            {
                Encoding = EncodingUtil.GetEncoding(lyricEncoding),
                WriteTimeLine = options.GetValueAsBoolean("timeline", true)
            };
            lrcFile.Write(path, writingSettings);
        }
    }
}