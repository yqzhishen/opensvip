﻿using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace OpenSvip.Stream
{
    public class JsonSvipConverter : IProjectConverter
    {
        public bool Indented { get; set; }
        
        public JsonSvipConverter() { }

        public JsonSvipConverter(bool indented)
        {
            Indented = indented;
        }

        public Project Load(string path)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, new UTF8Encoding(false));
            var project = JsonConvert.DeserializeObject<Project>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            stream.Dispose();
            reader.Dispose();
            return project;
        }

        public void Save(string path, Project project)
        {
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, new UTF8Encoding(false));
            var settings = new JsonSerializerSettings
            {
                Formatting = Indented ? Formatting.Indented : Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Error
            };
            writer.Write(JsonConvert.SerializeObject(project, settings));
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
            writer.Dispose();
            stream.Dispose();
        }
    }
}