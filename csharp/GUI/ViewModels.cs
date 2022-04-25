using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using OpenSvip.Framework;
using OpenSvip.GUI.Config;

namespace OpenSvip.GUI
{
    public class AppModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /* Use this to allow multiple threads to visit an ObservableCollection.
        private readonly object _lock = new object();

        public AppModel()
        {
            BindingOperations.EnableCollectionSynchronization(TaskList, _lock);
        }
        */
        public AppModel()
        {
            InputOptions = Plugins.ConvertAll(plugin =>
            {
                ObservableCollection<OptionViewModel> collection = new AsyncObservableCollection<OptionViewModel>();
                foreach (var option in plugin.InputOptions)
                {
                    collection.Add(new OptionViewModel(option));
                }
                return collection;
            });
            OutputOptions = Plugins.ConvertAll(plugin =>
            {
                ObservableCollection<OptionViewModel> collection = new AsyncObservableCollection<OptionViewModel>();
                foreach (var option in plugin.OutputOptions)
                {
                    collection.Add(new OptionViewModel(option));
                }
                return collection;
            });
        }

        public Information Information { get; set; } = new Information();

        public List<Plugin> Plugins { get; set; } = PluginManager.GetAllPlugins().ToList();

        public List<string> Formats => Plugins.ConvertAll(plugin => $"{plugin.Format} (*.{plugin.Suffix})");

        private bool _autoDetectFormat;

        public bool AutoDetectFormat
        {
            get => _autoDetectFormat;
            set
            {
                _autoDetectFormat = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoDetectFormat"));
            }
        }

        private bool _autoResetTasks;

        public bool AutoResetTasks
        {
            get => _autoResetTasks;
            set {
                _autoResetTasks = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoResetTasks"));
            }
        }

        private bool _autoExtension;

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

        private bool _openExportFolder;

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

        private ExportPaths _defaultExportPath;

        public ExportPaths DefaultExportPath
        {
            get => _defaultExportPath;
            set {
                _defaultExportPath = value;
                switch (value)
                {
                    case ExportPaths.Source:
                        ExportPath.PathValue = "${src}";
                        break;
                    case ExportPaths.Desktop:
                        ExportPath.PathValue = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        break;
                    case ExportPaths.Custom:
                        // TODO: implement this
                        break;
                    case ExportPaths.Unset:
                    default:
                        break;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DefaultExportPath"));
            }
        }

        public ObservableCollection<CustomPath> CustomExportPaths { get; set; } = new AsyncObservableCollection<CustomPath>();

        private int _selectedCustomExportPathIndex = -1;

        public int SelectedCustomExportPathIndex
        {
            get => _selectedCustomExportPathIndex;
            set
            {
                _selectedCustomExportPathIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedCustomExportPathIndex"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedCustomExportPath"));
            }
        }

        public CustomPath SelectedCustomExportPath => SelectedCustomExportPathIndex >= 0
            ? CustomExportPaths[SelectedCustomExportPathIndex]
            : null;

        private int _selectedInputPluginIndex = -1;

        public int SelectedInputPluginIndex
        {
            get => _selectedInputPluginIndex;
            set
            {
                _selectedInputPluginIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedInputPluginIndex"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedInputPlugin"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedInputFormat"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedInputOptions"));
            }
        }

        public Plugin SelectedInputPlugin
            => SelectedInputPluginIndex >= 0 ? Plugins[SelectedInputPluginIndex] : null;

        public string SelectedInputFormat =>
            SelectedInputPluginIndex >= 0 ? Plugins[SelectedInputPluginIndex].Format : null;

        public ObservableCollection<OptionViewModel> SelectedInputOptions
            => _selectedInputPluginIndex >= 0 ? InputOptions[_selectedInputPluginIndex] : null;

        public List<ObservableCollection<OptionViewModel>> InputOptions { get; }

        private int _selectedOutputPluginIndex = -1;

        public int SelectedOutputPluginIndex
        {
            get => _selectedOutputPluginIndex;
            set
            {
                _selectedOutputPluginIndex = value;
                ExportExtension = AutoExtension && value >= 0 ? "." + SelectedOutputPlugin.Suffix : null;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOutputPluginIndex"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOutputPlugin"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOutputFormat"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOutputOptions"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportExtension"));
            }
        }

        public Plugin SelectedOutputPlugin
            => SelectedOutputPluginIndex >= 0 ? Plugins[SelectedOutputPluginIndex] : null;

        public string SelectedOutputFormat =>
            SelectedOutputPluginIndex >= 0 ? Plugins[SelectedOutputPluginIndex].Format : null;

        public ObservableCollection<OptionViewModel> SelectedOutputOptions
            => _selectedOutputPluginIndex >= 0 ? OutputOptions[_selectedOutputPluginIndex] : null;

        public List<ObservableCollection<OptionViewModel>> OutputOptions { get; }

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
                        task.ExportTitle = Path.GetFileNameWithoutExtension(task.ImportFilename);
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

        public ObservableCollection<TaskViewModel> TaskList { get; set; } = new AsyncObservableCollection<TaskViewModel>();

        public CustomPath ExportPath { get; set; } = new CustomPath();

        private bool _executionInProgress;

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

    public class TaskViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize()
        {
            Status = TaskStates.Ready;
            Warnings.Clear();
            Error = null;
        }

        public void PrepareForExecution()
        {
            Status = TaskStates.Queued;
            Warnings.Clear();
            Error = null;
        }

        public string ImportFilename => Path.GetFileName(ImportPath);

        public string ImportDirectory => Path.GetDirectoryName(ImportPath);

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

        public string ExportFolder { get; set; }

        public string ExportPath { get; set; }

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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Error"));
            }
        }
    }

    public enum TaskStates
    {
        Ready, Queued, Success, Warning, Error, Skipped
    }

    public class OptionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public OptionViewModel(Option option)
        {
            OptionInfo = option;
            OptionValue = option.Default;
            if (OptionInfo.Type == "enum")
            {
                for (var i = 0; i < OptionInfo.EnumChoices.Length; ++i)
                {
                    if (OptionInfo.EnumChoices[i].Tag != option.Default)
                    {
                        continue;
                    }
                    _choiceIndex = i;
                    break;
                }
            }
        }

        public Option OptionInfo { get; set; }

        private string _optionValue;

        public string OptionValue
        {
            get
            {
                if (OptionInfo.Type == "enum")
                {
                    return ChoiceIndex >= 0 ? OptionInfo.EnumChoices[ChoiceIndex].Tag : null;
                }
                return _optionValue;
            }
            set
            {
                _optionValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OptionValue"));
            }
        }

        private int _choiceIndex = -1;

        public int ChoiceIndex
        {
            get => _choiceIndex;
            set
            {
                _choiceIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChoiceIndex"));
            }
        }
    }

    public class CustomPath : INotifyPropertyChanged
    {
        private const string SOURCE_TAG = "${src}";

        public event PropertyChangedEventHandler PropertyChanged;
        
        private string _pathValue = "";

        public string PathValue
        {
            get => _pathValue;
            set
            {
                _pathValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PathValue"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRelative"));
            }
        }

        public bool IsRelative => _pathValue == SOURCE_TAG || _pathValue.StartsWith(SOURCE_TAG + Path.PathSeparator) || _pathValue.StartsWith(SOURCE_TAG + '/');

        public string ActualValue => IsRelative ? _pathValue.Substring(7) : _pathValue;
    }
}
