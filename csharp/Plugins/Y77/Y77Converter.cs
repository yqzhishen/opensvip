using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace FlutyDeer.Y77Plugin
{
    internal class Y77Converter : IProjectConverter
    {

        public Project Load(string path, ConverterOptions options)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var Y77Project = JsonConvert.DeserializeObject<Y77Project>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            return new Y77Decoder().DecodeProject(Y77Project);
            //return new Project();
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var Y77Project = new Y77Encoder().EncodeProject(project);
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, Encoding.UTF8);
            var jsonString = JsonConvert.SerializeObject(Y77Project);
            foreach (var ch in jsonString)
            {
                if (ch > 32 && ch < 127)
                {
                    writer.Write(ch);
                }
                else
                {
                    writer.Write($@"\u{(int)ch:x4}");
                }
            }
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
        }

    }
}