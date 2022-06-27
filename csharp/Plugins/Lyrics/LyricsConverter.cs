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
            var lrcString = new LyricsEncoder().EncodeProject(project);
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, new UTF8Encoding(true));
            foreach (var ch in lrcString)
            {
                writer.Write(ch);
            }
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
        }
    }

}