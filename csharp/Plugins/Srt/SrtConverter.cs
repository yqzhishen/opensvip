using FlutyDeer.SrtPlugin.Options;
using FlutyDeer.SrtPlugin.Utils;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;

namespace FlutyDeer.SrtPlugin.Stream
{
    public class SrtConverter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            throw new NotImplementedException("不支持将本插件作为输入端。");
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var srtFile = new SrtEncoder
            {
                Offset = options.GetValueAsInteger("offset"),
                SplitByOption = options.GetValueAsEnum("splitBy", SplitByOption.Both)
            }.EncodeProject(project);
            var lyricEncoding = options.GetValueAsEnum("encoding", LyricEncodingOption.UTF8);
            var writingSettings = new WritingSettings
            {
                Encoding = EncodingUtil.GetEncoding(lyricEncoding)
            };
            srtFile.Write(path, writingSettings);
        }
    }
}