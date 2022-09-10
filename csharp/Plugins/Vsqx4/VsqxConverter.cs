using System;
using System.IO;
using System.Xml.Serialization;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace Plugin.VSQX
{
    public class VsqxConverter:IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            return VsqxDecoder.Load(path);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            throw new Exception("不支持保存为Vsqx");
        }
    }
}