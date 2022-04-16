using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using OpenSvip.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenSvip.GUI
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
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

        public string Version { get; set; } = "1.0.1 (Preview)";

        public string FrameworkVersion { get; set; } = "1.2.0";

        public string Author { get; set; } = "YQ之神";

        public string AuthorHomePage { get; set; } = "https://space.bilibili.com/102844209";

        public string GitHubRepository { get; set; } = "https://github.com/yqzhishen/opensvip";

        public List<Plugin> Plugins { get; } = PluginManager.GetAllPlugins().ToList();

        public List<string> Formats
        {
            get => Plugins.ConvertAll(plugin => $"{plugin.Format} (*.{plugin.Suffix})");
        }

        private bool _autoDetectFormat = true;

        [JsonProperty]
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

        [JsonProperty]
        public bool AutoResetTasks
        {
            get => _autoResetTasks;
            set {
                _autoResetTasks = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoResetTasks"));
            }
        }

        private bool _autoExtension = true;

        [JsonProperty]
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

        [JsonProperty]
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

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public OverwriteOptions OverWriteOption
        {
            get => _overwriteOption;
            set
            {
                _overwriteOption = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OverwriteOptions"));
            }
        }

        private DefaultExport _defaultExportPath = DefaultExport.None;

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public DefaultExport DefaultExportPath
        {
            get => _defaultExportPath;
            set {
                _defaultExportPath = value;
                switch (value)
                {
                    case DefaultExport.Source:
                        ExportPath = "";
                        break;
                    case DefaultExport.Desktop:
                        ExportPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        break;
                    case DefaultExport.Custom:
                        // TODO: implement this
                        break;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DefaultExport"));
            }
        }

        [JsonProperty]
        public ObservableCollection<string> CustomExportPaths { get; set; } = new AsyncObservableCollection<string>();

        private int _selectedInputPluginIndex = -1;

        public int SelectedInputPluginIndex
        {
            get => _selectedInputPluginIndex;
            set
            {
                _selectedInputPluginIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedInputPluginIndex"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedInputPlugin"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedInputOptions"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOutputOptions"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExportExtension"));
            }
        }

        public Plugin SelectedOutputPlugin
        {
            get => SelectedOutputPluginIndex >= 0 ? Plugins[SelectedOutputPluginIndex] : null;
        }

        public List<ObservableCollection<OptionViewModel>> InputOptions { get; }

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

        public ObservableCollection<OptionViewModel> SelectedInputOptions
        {
            get => _selectedInputPluginIndex >= 0 ? InputOptions[_selectedInputPluginIndex] : null;
        }
        
        public List<ObservableCollection<OptionViewModel>> OutputOptions { get; }

        public ObservableCollection<OptionViewModel> SelectedOutputOptions
        {
            get => _selectedOutputPluginIndex >= 0 ? OutputOptions[_selectedOutputPluginIndex] : null;
        }

        public ObservableCollection<TaskViewModel> TaskList { get; set; } = new AsyncObservableCollection<TaskViewModel>();

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

        public string ExportFolder { get; set; }

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
                    if (OptionInfo.EnumChoices[i].Tag == option.Default)
                    {
                        _choiceIndex = i;
                        break;
                    }
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

    public enum TaskStates
    {
        Ready, Queued, Success, Warning, Error, Skipped
    }

    public enum DefaultExport
    {
        None, Source, Desktop, Custom 
    }

    public enum OverwriteOptions
    {
        Overwrite, Skip, Ask
    }
}
