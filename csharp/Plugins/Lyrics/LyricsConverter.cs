using FlutyDeer.LyricsPlugin.Options;
using FlutyDeer.LyricsPlugin.Utils;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlutyDeer.LyricsPlugin.Stream
{
    internal class LyricsConverter : IProjectConverter
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
            }.EncodeProject(project);
            var lyricEncoding = options.GetValueAsEnum("encoding", LyricEncodingOption.UTF8);
            var writingSettings = new WritingSettings
            {
                Encoding = EncodingUtil.GetEncoding(lyricEncoding)
            };
            lrcFile.Write(path, writingSettings);
        }
    }

}