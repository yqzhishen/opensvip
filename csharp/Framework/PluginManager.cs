using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace OpenSvip.Framework
{
    public static class PluginManager
    {
        private static readonly string PluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Plugins\");

        private static readonly Dictionary<string, Plugin> Plugins = new Dictionary<string, Plugin>();

        static PluginManager()
        {
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
                    Plugins[plugin.Identifier] = plugin;
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    stream.Close();
                    reader.Close();
                }
            }
        }

        public static IProjectConverter GetConverter(string identifier)
        {
            var plugin = GetPlugin(identifier);
            var assembly = Assembly.LoadFile(Path.Combine(PluginPath, plugin.LibraryPath));
            var type = assembly.GetType(plugin.Converter);
            return (IProjectConverter) Activator.CreateInstance(type);
        }

        public static Plugin GetPlugin(string identifier)
        {
            if (!Plugins.ContainsKey(identifier))
            {
                throw new ArgumentException("找不到标识符所对应的转换插件。");
            }
            return Plugins[identifier];
        }

        public static Plugin[] GetAllPlugins()
        {
            return Array.ConvertAll(Plugins.ToArray(), kv => kv.Value);
        }
    }
}
