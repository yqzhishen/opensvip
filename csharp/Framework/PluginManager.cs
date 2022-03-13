using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenSvip.Framework
{
    public static class PluginManager
    {

        private static readonly string PluginPath = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";

        private static readonly Dictionary<string, Plugin> Plugins = new Dictionary<string, Plugin>();
        
        static PluginManager()
        {
            foreach (var plugin in ApplicationInfo.Application.Plugins)
            {
                Plugins[plugin.Identifier] = plugin;
            }
        }
        
        public static IProjectConverter GetConverter(string identifier)
        {
            var plugin = GetPlugin(identifier);
            var assembly = Assembly.LoadFile(PluginPath + plugin.LibraryPath);
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
    }
}
