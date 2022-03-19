using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.Win32;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace OpenSvip.Stream
{
    public class BinarySvipConverter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            var reader = new FileStream(path, FileMode.Open, FileAccess.Read);
            var buffer = new byte[9];
            reader.ReadByte();
            reader.Read(buffer, 0, 4);
            reader.ReadByte();
            reader.Read(buffer, 4, 5);
            var version = Encoding.Default.GetString(buffer);
            var model = (SingingTool.Model.AppModel) new BinaryFormatter().Deserialize(reader);
            reader.Close();
            return new BinarySvipDecoder().DecodeProject(version, model);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var (version, model) = new BinarySvipEncoder().EncodeProject(project);
            var verEnum = options.GetOptionAsEnum<BinarySvipVersions>("version");
            switch (verEnum)
            {
                case BinarySvipVersions.SVIP7_0_0:
                    version = "SVIP7.0.0";
                    break;
                case BinarySvipVersions.SVIP6_0_0:
                    version = "SVIP6.0.0";
                    break;
                case BinarySvipVersions.Automatic:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var buffer = Encoding.Default.GetBytes(version);
            var writer = new FileStream(path, FileMode.Create, FileAccess.Write);
            writer.WriteByte(4);
            writer.Write(buffer, 0, 4);
            writer.WriteByte(5);
            writer.Write(buffer, 4, 5);
            new BinaryFormatter().Serialize(writer, model);
            writer.Flush();
            writer.Close();
        }
        
        static BinarySvipConverter()
        {
            AppDomain.CurrentDomain.AssemblyResolve += SingingToolResolveEventHandler;
        }
        
        private static string FindLibrary()
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\svipfile\shell\open\command");
            if (key == null)
            {
                throw new FileNotFoundException("未检测到已安装的 X Studio · 歌手软件。");
            }
            var value = key.GetValue("").ToString().Split('"')[1];
            return value.Substring(0, value.Length - 18);
        }

        private static Assembly SingingToolResolveEventHandler(object sender, ResolveEventArgs args)
        {
            var path = FindLibrary();
            var filename = args.Name.Split(',')[0];
            return Assembly.LoadFile($@"{path}\{filename}.dll");
        }
    }
}
