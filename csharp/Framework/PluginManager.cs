using System;
using System.Collections.Generic;
using System.IO;
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
                try
                {
                    var stream = new FileStream(Path.Combine(pluginDir, "Properties.xml"), FileMode.Open, FileAccess.Read);
                    var reader = new StreamReader(stream, new UTF8Encoding(false));
                    var plugin = (Plugin) new XmlSerializer(typeof(Plugin)).Deserialize(reader);
                    stream.Close();
                    reader.Close();
                    stream.Dispose();
                    reader.Dispose();
                    Plugins[plugin.Identifier] = plugin;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public static IProjectConverter GetConverter(string identifier)
        {
            var plugin = GetPluginProperties(identifier);
            var assembly = Assembly.LoadFile(PluginPath + plugin.LibraryPath);
            var type = assembly.GetType(plugin.Converter);
            return (IProjectConverter) Activator.CreateInstance(type);
        }

        public static Plugin GetPluginProperties(string identifier)
        {
            if (!Plugins.ContainsKey(identifier))
            {
                throw new ArgumentException("找不到标识符所对应的转换插件。");
            }
            return Plugins[identifier];
        }
    }
}
