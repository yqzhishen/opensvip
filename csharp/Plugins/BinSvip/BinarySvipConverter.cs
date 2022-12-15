using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static readonly string[] _checkList =
        {
            "SingingTool.Const.dll",
            "SingingTool.Library.dll",
            "SingingTool.Model.dll",
            "Newtonsoft.Json.dll"
        };

        private static readonly List<string> _libraryDirectories = new List<string>();

        public Project Load(string path, ConverterOptions options)
        {
            if (options.GetValueAsBoolean("independentMode"))
            {
                return new Standalone.NrbfSvipConverter().Load(path, options);
            }

            CheckForLibraries(options.GetValueAsString("libraryPath"));
            AppDomain.CurrentDomain.AssemblyResolve += ExternalAssemblyResolveEventHandler;
            return DoLoad(path);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            if (options.GetValueAsBoolean("independentMode"))
            {
                new Standalone.NrbfSvipConverter().Save(path, project, options);
                return;
            }

            CheckForLibraries(options.GetValueAsString("libraryPath"));
            AppDomain.CurrentDomain.AssemblyResolve += ExternalAssemblyResolveEventHandler;
            DoSave(path, project, options);
        }

        private Project DoLoad(string path)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(stream);
            var version = reader.ReadString();
            var versionNumber = reader.ReadString();
            version += versionNumber;
            SingingTool.Model.AppModel model;
            if (version != "0.0.0" && new Version(versionNumber) < new Version("2.0.0"))
            {
                stream.Close();
                model = SingingTool.Model.ProjectModelFileMgr.ReadModelFile(path, out _, out _);
            }
            else
            {
                model = (SingingTool.Model.AppModel)new BinaryFormatter().Deserialize(stream);
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
                case BinarySvipVersions.Automatic:
                    if (version != "SVIP0.0.0")
                    {
                        break;
                    }

                    goto case BinarySvipVersions.SVIP6_0_0;
                case BinarySvipVersions.SVIP6_0_0:
                    version = "SVIP6.0.0";
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

        private void CheckForLibraries(string specifiedDir = null)
        {
            var foundLibraries = new HashSet<string>();

            if (!string.IsNullOrWhiteSpace(specifiedDir) && Directory.Exists(specifiedDir))
            {
                foundLibraries.UnionWith(Directory.GetFiles(specifiedDir).Select(Path.GetFileName));
                _libraryDirectories.Add(specifiedDir);
            }

            var selfDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(selfDir))
            {
                foundLibraries.UnionWith(Directory.GetFiles(selfDir).Select(Path.GetFileName));
                _libraryDirectories.Add(selfDir);
            }

            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\svipfile\shell\open\command");
            var value = key?.GetValue("").ToString().Split('"')[1];
            var xstudioDir = Path.GetDirectoryName(value);
            if (Directory.Exists(xstudioDir))
            {
                foundLibraries.UnionWith(Directory.GetFiles(xstudioDir).Select(Path.GetFileName));
                _libraryDirectories.Add(xstudioDir);
            }

            if (!foundLibraries.IsSupersetOf(_checkList))
            {
                throw new FileNotFoundException("缺少必要的动态链接库，可能是因为未安装 X Studio · 歌手软件。请查看插件依赖说明。");
            }
        }

        private static Assembly ExternalAssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            var filename = args.Name.Split(',')[0];
            foreach (var directory in _libraryDirectories)
            {
                try
                {
                    return Assembly.LoadFrom(Path.Combine(directory, filename + ".dll"));
                }
                catch
                {
                    // ignored
                }
            }

            throw new FileNotFoundException();
        }
    }
}