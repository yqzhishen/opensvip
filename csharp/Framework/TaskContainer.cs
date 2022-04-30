using System;
using System.IO;
using System.Reflection;

namespace OpenSvip.Framework
{
    public class TaskContainer : MarshalByRefObject
    {
        private IProjectConverter _inputConverter;

        private IProjectConverter _outputConverter;

        private ConverterOptions _inputOptions;

        private ConverterOptions _outputOptions;

        private static IProjectConverter LoadConverter(Plugin plugin)
        {
            var assembly = Assembly.LoadFrom(Path.Combine(PluginManager.PluginPath, plugin.LibraryPath));
            var type = assembly.GetType(plugin.Converter);
            return (IProjectConverter) Activator.CreateInstance(type);
        }
        
        public void Init(
            Plugin inputPlugin,
            Plugin outputPlugin,
            ConverterOptions inputOptions,
            ConverterOptions outputOptions)
        {
            _inputConverter = LoadConverter(inputPlugin);
            _outputConverter = LoadConverter(outputPlugin);
            _inputOptions = inputOptions;
            _outputOptions = outputOptions;
        }

        public void Run(string importPath, string exportPath)
        {
            Warnings.AddWarning("Hello!");
            _outputConverter.Save(exportPath, _inputConverter.Load(importPath, _inputOptions), _outputOptions);
        }

        public Warning[] GetWarnings()
        {
            return Warnings.GetWarnings();
        }

        public void ClearWarnings()
        {
            Warnings.ClearWarnings();
        }
    }
}
