using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace OpenSvip.Stream
{
    public class BinarySvipConverter : IProjectConverter
    {
        
        private static string libraryPath;
        
        public Project Load(string path, ConverterOptions options)
        {
            libraryPath = FindLibrary();
            AppDomain.CurrentDomain.AssemblyResolve += SingingToolResolveEventHandler;
            return DoLoad(path);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            libraryPath = FindLibrary();
            AppDomain.CurrentDomain.AssemblyResolve += SingingToolResolveEventHandler;
            DoSave(path, project, options);
        }

        private Project DoLoad(string path)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(stream);
            var version = reader.ReadString() + reader.ReadString();
            SingingTool.Model.AppModel model;
            if (version != "SVIP0.0.0")
            {
                stream.Close();
                model = SingingTool.Model.ProjectModelFileMgr.ReadModelFile(path, out _, out _);
            }
            else
            {
                model = (SingingTool.Model.AppModel) new BinaryFormatter().Deserialize(stream);
                stream.Close();
            }
            reader.Close();
            return new BinarySvipDecoder().DecodeProject(version, model);
        }
        
        private void DoSave(string path, Project project, ConverterOptions options)
        {
            var (version, model) = new BinarySvipEncoder
            {
                DefaultSinger = options.GetValueAsString("singer", "陈水若"),
                DefaultTempo = options.GetValueAsInteger("tempo", 60)
            }.EncodeProject(project);
            var verEnum = options.GetValueAsEnum<BinarySvipVersions>("version");
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
                case BinarySvipVersions.Compatible:
                    version = "SVIP0.0.0";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var index = Regex.Match(version, @"\d+\.\d+\.\d+").Groups[0].Index;
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new BinaryWriter(stream);
            writer.Write(version.Substring(0, index));
            writer.Write(version.Substring(index, version.Length - index));
            writer.Flush();
            new BinaryFormatter().Serialize(stream, model);
            stream.Flush();
            writer.Close();
            stream.Close();
        }
        
        private static string FindLibrary()
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\svipfile\shell\open\command");
            var value = key?.GetValue("").ToString().Trim('"');
            if (!File.Exists(value))
            {
                throw new FileNotFoundException("未检测到已安装的 X Studio · 歌手软件。请查看插件依赖说明。");
            }
            return Path.GetDirectoryName(value);
        }

        private static Assembly SingingToolResolveEventHandler(object sender, ResolveEventArgs args)
        {
            var filename = args.Name.Split(',')[0];
            return Assembly.LoadFrom(Path.Combine(libraryPath, filename + ".dll"));
        }
    }
}
