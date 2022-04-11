using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using OpenSvip.Framework;

namespace OpenSvip.GUI
{
    public class AppModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static AppModel Model { get; private set; } = new AppModel();

        public List<Plugin> Plugins { get; set; } = PluginManager.GetAllPlugins().ToList();

        public List<string> Formats
        {
            get => Plugins.ConvertAll(plugin => $"{plugin.Format} (*.{plugin.Suffix})");
        }

        private bool _autoResetTasks = true;

        public bool AutoResetTasks
        {
            get => _autoResetTasks;
            set {
                _autoResetTasks = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoResetTasks"));
            }
        }

        private bool _autoDetectFormat = true;

        public bool AutoDetectFormat
        {
            get => _autoDetectFormat;
            set
            {
                _autoDetectFormat = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoDetectFormat"));
            }
        }

        private bool _openExportFolder = false;

        public bool OpenExportFolder
        {
            get => _openExportFolder;
            set
            {
                _openExportFolder = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpenExportFolder"));
            }
        }

        private OverwriteOptions _overwriteOption = OverwriteOptions.Overwrite;

        public OverwriteOptions OverWriteOption
        {
            get => _overwriteOption;
            set
            {
                _overwriteOption = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OverwriteOptions"));
            }
        }

        private int _selectedInputPluginIndex = -1;

        public int SelectedInputPluginIndex
        {
            get => _selectedInputPluginIndex;
            set
            {
                _selectedInputPluginIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedInputPluginIndex"));
            }
        }

        public int _selectedOutputPluginIndex = -1;

        public int SelectedOutputPluginIndex
        {
            get => _selectedOutputPluginIndex;
            set
            {
                _selectedOutputPluginIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOutputPluginIndex"));
            }
        }

        public ObservableCollection<Task> TaskList { get; set; } = new ObservableCollection<Task>();

        private string _exportPath;

        public string ExportPath
        {
            get => _exportPath;
            set
            {
                _exportPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportPath"));
            }
        }

        private bool _executionInProgress = false;

        public bool ExecutionInProgress
        {
            get => _executionInProgress;
            set
            {
                _executionInProgress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExecutionInProgress"));
            }
        }
    }

    public class Task
    {
        public Plugin InputPlugin { get; set; }
        
        public Plugin OutputPlugin { get; set; }

        public string ImportFilename
        {
            get => Path.GetFileName(ImportPath);
        }

        public string ImportDirectory
        {
            get => Path.GetDirectoryName(ImportPath);
        }

        public string ImportPath { get; set; }

        public string ExportPath { get; set; }

        public string Status { get; set; }
    }

    public enum OverwriteOptions
    {
        Overwrite, Skip, Ask
    }
}
