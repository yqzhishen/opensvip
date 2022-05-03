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
                rect => new TomlArray { rect.Left, rect.Top, rect.Width, rect.Height },
                tomlValue =>
                {
                    if (!(tomlValue is TomlArray tomlArray))
                    {
                        return new Rect();
                    }
                    try
                    {
                        var arr = tomlArray.Select(val => ((TomlDouble)val).Value).ToArray();
                        return new Rect(arr[0], arr[1], arr[2], arr[3]);
                    }
                    catch
                    {
                        return new Rect();
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

    public class Information
    {
        public string Version { get; set; } = "1.1.3 (Preview)";

        public string FrameworkVersion { get; set; } = ConstValues.FrameworkVersion;

        public string AuthorHomePage { get; set; } = "https://space.bilibili.com/102844209";

        public string GitHubRepository { get; set; } = "https://github.com/yqzhishen/opensvip";
    }

    [TomlDoNotInlineObject]
    public class Properties
    {
        public Rect MainRestoreBounds { get; set; } = new Rect();

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

        public AppearanceThemes AppearanceTheme { get; set; } = AppearanceThemes.System;
    }

    public class PathConfig
    {
        public string Path { get; set; }

        public bool Selected { get; set; }
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
