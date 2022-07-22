using System;
using System.IO;
using System.Linq;
using System.Windows;
using OpenSvip.Framework;
using Tomlet;
using Tomlet.Attributes;
using Tomlet.Models;

namespace OpenSvip.GUI.Config
{
    public class AppConfig
    {
        private const string CONFIG_FOLDER = "Converter";

        private const string CONFIG_FILENAME = "Configurations.toml";

        private static readonly string ActualConfigFolder = Path.Combine(ConstValues.CommonConfigPath, CONFIG_FOLDER);

        private static readonly string ActualConfigFile = Path.Combine(ActualConfigFolder, CONFIG_FILENAME);

        [TomlProperty("Restoration")]
        public Properties Properties { get; set; } = new Properties();

        [TomlProperty("Preference")]
        public Settings Settings { get; set; } = new Settings();

        static AppConfig()
        {
            TomletMain.RegisterMapper(
                tuple => new TomlArray { tuple.Item1, tuple.Item2 },
                tomlValue =>
                {
                    if (!(tomlValue is TomlArray tomlArray))
                    {
                        return new Tuple<double, double>(-1, -1);
                    }
                    try
                    {
                        var arr = tomlArray.Select(val => ((TomlDouble)val).Value).ToArray();
                        return new Tuple<double, double>(arr[0], arr[1]);
                    }
                    catch
                    {
                        return new Tuple<double, double>(-1, -1);
                    }
                });
        }

        public static AppConfig LoadFromFile()
        {
            try
            {
                var stream = new FileStream(ActualConfigFile, FileMode.Open, FileAccess.Read);
                var reader = new StreamReader(stream);
                var config = TomletMain.To<AppConfig>(reader.ReadToEnd());
                reader.Close();
                stream.Close();
                return config;
            }
            catch
            {
                return new AppConfig();
            }
        }

        public void SaveToFile()
        {
            try
            {
                Directory.CreateDirectory(ActualConfigFolder);
                var stream = new FileStream(ActualConfigFile, FileMode.Create, FileAccess.Write);
                var writer = new StreamWriter(stream);
                writer.Write(TomletMain.TomlStringFrom(this));
                writer.Flush();
                stream.Flush();
                writer.Close();
                stream.Close();
            }
            catch
            {
                // ignored
            }
        }
    }

    public static class Information
    {
        public const string ApplicationVersion = "1.4.1";

        public const string OnlineDocuments = "https://openvpi.github.io/docs/guide.html";

        public const string GitHubRepository = "https://github.com/yqzhishen/opensvip";

        public const string UpdateLogUrl = "https://openvpi.github.io/updatelogs/converter.toml";
    }

    [TomlDoNotInlineObject]
    public class Properties
    {
        public Tuple<double, double> MainWindowSize { get; set; } = new Tuple<double, double>(-1, -1);

        public WindowState MainWindowState { get; set; } = WindowState.Normal;
    }

    public class Settings
    {
        public string ImportPluginId { get; set; }

        public string ExportPluginId { get; set; }

        public bool AutoDetectFormat { get; set; } = true;

        public bool AutoResetTasks { get; set; } = true;

        public bool AutoExtension { get; set; } = true;

        public bool OpenExportFolder { get; set; } = false;

        public OverwriteOptions OverwriteOption { get; set; } = OverwriteOptions.Overwrite;

        public ExportPaths DefaultExportPath { get; set; } = ExportPaths.Unset;

        public PathConfig[] CustomExportPaths { get; set; } = Array.Empty<PathConfig>();

        public string LastExportPath { get; set; }

        public bool EnableMultiThreading { get; set; } = true;

        public AppearanceThemes AppearanceTheme { get; set; } = AppearanceThemes.System;

        public bool CheckForUpdates { get; set; } = true;
    }

    public class PathConfig
    {
        public string Path { get; set; }

        public bool Selected { get; set; }
    }

    public class UpdateLog
    {
        public string Version { get; set; }

        public string RequiredFrameworkVersion { get; set; } = "1.0.0";

        public string Date { get; set; }

        public string DownloadLink { get; set; }

        public string Prologue { get; set; }

        public string[] Items { get; set; } = Array.Empty<string>();

        public string Epilogue { get; set; }
    }

    public enum OverwriteOptions
    {
        Overwrite, Skip, Ask
    }

    public enum ExportPaths
    {
        Unset, Source, Desktop, Custom
    }

    public enum AppearanceThemes
    {
        Light, Dark, System
    }
}
