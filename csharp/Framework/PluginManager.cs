using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace OpenSvip.Framework
{
    public static class PluginManager
    {
        public static readonly string PluginPath = Path.Combine(ConstValues.CommonDataPath, "Plugins");

        public static readonly string TempPath = Path.Combine(ConstValues.CommonDataPath, "Temp");

        private static readonly Dictionary<string, Plugin> Plugins = new Dictionary<string, Plugin>();

        private static readonly Dictionary<string, string> Folders = new Dictionary<string, string>();

        static PluginManager()
        {
            try
            {
                Directory.CreateDirectory(PluginPath);
                foreach (var pluginDir in Directory.GetDirectories(PluginPath))
                {
                    var propertiesPath = Path.Combine(pluginDir, "Properties.xml");
                    if (!File.Exists(propertiesPath))
                    {
                        continue;
                    }

                    var stream = new FileStream(propertiesPath, FileMode.Open, FileAccess.Read);
                    var reader = new StreamReader(stream, new UTF8Encoding(false));
                    try
                    {
                        var plugin = (Plugin) new XmlSerializer(typeof(Plugin)).Deserialize(reader);
                        if (new Version(ConstValues.FrameworkVersion) < new Version(plugin.TargetFramework))
                        {
                            continue;
                        }
                        Plugins[plugin.Identifier] = plugin;
                        Folders[plugin.Identifier] = pluginDir;
                    }
                    finally
                    {
                        stream.Close();
                        reader.Close();
                    }
                }
            }
            catch (IOException)
            {
                // ignored
            }
        }

        public static IProjectConverter GetConverter(string identifier)
        {
            var plugin = GetPlugin(identifier);
            var assembly = Assembly.LoadFrom(Path.Combine(PluginPath, plugin.LibraryPath));
            var type = assembly.GetType(plugin.Converter);
            return (IProjectConverter) Activator.CreateInstance(type);
        }

        public static bool HasPlugin(string identifier)
        {
            return Plugins.ContainsKey(identifier);
        }

        public static Plugin GetPlugin(string identifier)
        {
            if (!Plugins.ContainsKey(identifier))
            {
                throw new ArgumentException($"找不到标识符“{identifier}”所对应的转换插件。");
            }
            return Plugins[identifier];
        }

        public static Plugin[] GetAllPlugins()
        {
            return Array.ConvertAll(Plugins.OrderBy(pair => pair.Key).ToArray(), kv => kv.Value);
        }

        public static Plugin ExtractPlugin(string path, out string folder)
        {
            if (Directory.Exists(TempPath))
            {
                new DirectoryInfo(TempPath).Delete(true);
            }
            Directory.CreateDirectory(TempPath);
            ZipFile.ExtractToDirectory(path, TempPath);
            
            // iterate through all folders in TempPath unless not start with . or _
            // this is for osx's __MACOSX and .DS_Store

            folder = Directory.EnumerateDirectories(TempPath).First(
                dir => !Path.GetFileName(dir).StartsWith(".") && !Path.GetFileName(dir).StartsWith("_")
            );

            var propertiesPath = Path.Combine(folder, "Properties.xml");
            try
            {
                var stream = new FileStream(propertiesPath, FileMode.Open, FileAccess.Read);
                var reader = new StreamReader(stream, new UTF8Encoding(false));
                try
                {
                    return (Plugin)new XmlSerializer(typeof(Plugin)).Deserialize(reader);
                }
                finally
                {
                    stream.Close();
                    reader.Close();
                }
            }
            catch (IOException)
            {
                new DirectoryInfo(folder).Delete(true);
                throw new IOException($"压缩包“{path}”已损坏，或未包含正确的插件信息。");
            }
        }

        public static void InstallPlugin(Plugin plugin, string folder, bool force = false)
        {
            var folderName = Path.GetFileName(folder);
            var targetFolder = Path.Combine(PluginPath, folderName);
            if (Plugins.ContainsKey(plugin.Identifier)) // plugin already exists
            {
                new DirectoryInfo(Folders[plugin.Identifier]).Delete(true);
            }
            else // a new plugin
            {
                if (Directory.Exists(targetFolder))
                {
                    if (Folders.ContainsValue(targetFolder) && !force)
                    {
                        throw new PluginFolderConflictException(folderName);
                    }
                    new DirectoryInfo(targetFolder).Delete(true);
                }
            }
            Directory.Move(folder, targetFolder);
            Plugins[plugin.Identifier] = plugin;
            Folders[plugin.Identifier] = targetFolder;
        }
    }

    public class PluginFolderConflictException : IOException
    {
        public string FolderName { get; set; }

        public PluginFolderConflictException(string folderName)
        {
            FolderName = folderName;
        }
    }
}
