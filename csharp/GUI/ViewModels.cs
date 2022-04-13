using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using OpenSvip.Framework;

namespace OpenSvip.GUI
{
    public class AppModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /* Use this to allow multiple threads to visit an ObservableCollection
        private readonly object _lock = new object();

        public AppModel()
        {
            BindingOperations.EnableCollectionSynchronization(TaskList, _lock);
        }
        */
        public List<Plugin> Plugins { get; set; } = PluginManager.GetAllPlugins().ToList();

        public List<string> Formats
        {
            get => Plugins.ConvertAll(plugin => $"{plugin.Format} (*.{plugin.Suffix})");
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

        private bool _autoResetTasks = true;

        public bool AutoResetTasks
        {
            get => _autoResetTasks;
            set {
                _autoResetTasks = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoResetTasks"));
            }
        }

        private bool _autoExtension = true;

        public bool AutoExtension
        {
            get => _autoExtension;
            set {
                if (!value)
                {
                    ExportExtension = null;
                }
                else if (SelectedOutputPluginIndex >= 0)
                {
                    ExportExtension = "." + SelectedOutputPlugin.Suffix;
                }
                _autoExtension = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoExtension"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedInputPlugin"));
            }
        }

        public Plugin SelectedInputPlugin
        {
            get => SelectedInputPluginIndex >= 0 ? Plugins[SelectedInputPluginIndex] : null;
        }

        private int _selectedOutputPluginIndex = -1;

        public int SelectedOutputPluginIndex
        {
            get => _selectedOutputPluginIndex;
            set
            {
                _selectedOutputPluginIndex = value;
                ExportExtension = value >= 0 ? "." + SelectedOutputPlugin.Suffix : null;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOutputPluginIndex"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOutputPlugin"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportExtension"));
            }
        }

        public Plugin SelectedOutputPlugin
        {
            get => SelectedOutputPluginIndex >= 0 ? Plugins[SelectedOutputPluginIndex] : null;
        }

        public ObservableCollection<Task> TaskList { get; set; } = new AsyncObservableCollection<Task>();

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

        private string _exportExtension;

        public string ExportExtension
        {
            get => _exportExtension;
            set
            {
                if (_exportExtension == null && value != null)
                {
                    foreach (var task in TaskList)
                    {
                        task.ExportTitle = Path.GetFileNameWithoutExtension(task.ExportTitle);
                    }
                }
                else if (_exportExtension != null && value == null)
                {
                    foreach (var task in TaskList)
                    {
                        if (ExportExtension != Path.GetExtension(task.ExportTitle))
                        {
                            task.ExportTitle += ExportExtension;
                        }
                    }
                }
                _exportExtension = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportExtension"));
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

        public bool DialogIsOpen = false;
    }

    public class Task : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void PrepareForExecution()
        {
            Status = TaskStates.Queued;
            Warnings.Clear();
            Error = null;
        }

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

        private string _exportTitle;

        public string ExportTitle
        {
            get => _exportTitle;
            set
            {
                _exportTitle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportTitle"));
            }
        }

        public string ImportPath { get; set; }

        private TaskStates _status;

        public TaskStates Status
        {
            get => _status;
            set
            {
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
            }
        }

        public ObservableCollection<Warning> Warnings { get; set; } = new AsyncObservableCollection<Warning>();

        private string _error;

        public string Error
        {
            get => _error;
            set
            {
                _error = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
            }
        }
    }

    public enum OverwriteOptions
    {
        Overwrite, Skip, Ask
    }

    public enum TaskStates
    {
        Ready, Queued, Success, Warning, Error, Skipped
    }
}
