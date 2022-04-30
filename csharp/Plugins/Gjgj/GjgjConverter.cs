using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;
using Plugin.Gjgj;
using Gjgj.Model;
using System.Windows.Forms;
using System;

namespace Gjgj.Stream
{
    internal class GjgjConverter : IProjectConverter
    {
        /// <summary>
        /// 将歌叽歌叽工程转换为OpenSvip工程。
        /// </summary>
        /// <param name="path">输入路径。</param>
        /// <param name="options">输入选项。</param>
        /// <returns></returns>
        public Project Load(string path, ConverterOptions options)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var gjProject = JsonConvert.DeserializeObject<GjProject>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            return new GjgjDecoder().DecodeProject(gjProject);
        }

        /// <summary>
        /// 将OpenSvip工程转换为歌叽歌叽工程。
        /// </summary>
        /// <param name="path">输出路径。</param>
        /// <param name="project">OpenSvip工程。</param>
        /// <param name="options">输出选项。</param>
        public void Save(string path, Project project, ConverterOptions options)
        {
            var gjProject = new GjgjEncoder
            {
                ParamSampleInterval = options.GetValueAsInteger("desample", 32),
                SingerName = options.GetValueAsString("singer", "扇宝"),
            }.EncodeProject(project);
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, Encoding.UTF8);
            var jsonString = JsonConvert.SerializeObject(gjProject);
            foreach (var ch in jsonString)
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
