using System.Windows.Controls;
using System.Windows.Media.Animation;
using System;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using OpenSvip.Framework;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using OpenSvip.GUI.Config;
using OpenSvip.GUI.Dialog;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Microsoft.Toolkit.Uwp.Notifications;

namespace OpenSvip.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public AppModel Model { get; set; }
        
        private bool _skipSameFilename;

        private bool _askBeforeOverwrite;
        
        private Queue<TaskViewModel> _taskQueue;

        private readonly List<Thread> _converterThreads = new List<Thread>();

        private readonly object _overwriteDialogLock = new object();
        
        private readonly List<AppDomain> _appDomains = new List<AppDomain>();

        public MainWindow()
        {
            InitializeComponent();
            SystemEvents.UserPreferenceChanged += UserPreference_ThemeChanged;

            var config = AppConfig.LoadFromFile();

            var properties = config.Properties;
            var (width, height) = properties.MainWindowSize;
            WindowState = WindowState.Normal;
            if (width >= MinWidth)
            {
                Width = width;
            }
            if (height >= MinHeight)
            {
                Height = height;
            }
            WindowState = properties.MainWindowState;

            var settings = config.Settings;
            Model = new AppModel
            {
                AutoDetectFormat = settings.AutoDetectFormat,
                AutoResetTasks = settings.AutoResetTasks,
                AutoExtension = settings.AutoExtension,
                OpenExportFolder = settings.OpenExportFolder,
                OverWriteOption = settings.OverwriteOption,
                EnableMultiThreading = settings.EnableMultiThreading,
                AppearanceThemes = settings.AppearanceTheme,
                CheckForUpdates = settings.CheckForUpdates
            };
            Model.SelectedInputPluginIndex = settings.ImportPluginId == null ? -1 : Model.Plugins.FindIndex(plugin => plugin.Identifier.Equals(settings.ImportPluginId));
            Model.SelectedOutputPluginIndex = settings.ExportPluginId == null ? -1 : Model.Plugins.FindIndex(plugin => plugin.Identifier.Equals(settings.ExportPluginId));
            foreach (var path in settings.CustomExportPaths)
            {
                Model.CustomExportPaths.Add(new CustomPath
                {
                    PathValue = path.Path
                });
            }
            Model.SelectedCustomExportPathIndex = settings.DefaultExportPath == ExportPaths.Custom
                ? Array.FindIndex(settings.CustomExportPaths, path => path.Selected)
                : -1;
            if (settings.DefaultExportPath == ExportPaths.Custom && Model.SelectedCustomExportPathIndex == -1)
            {
                Model.DefaultExportPath = ExportPaths.Unset;
            }
            else
            {
                Model.DefaultExportPath = settings.DefaultExportPath;
            }
            if (Model.DefaultExportPath == ExportPaths.Unset && !string.IsNullOrWhiteSpace(settings.LastExportPath))
            {
                Model.ExportPath.PathValue = settings.LastExportPath;
            }
            DataContext = Model;
            if (Model.CheckForUpdates)
                BackgroundUpdateChecker.CheckPluginsUpdate();
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            AddConverterTasks(Environment.GetCommandLineArgs().Skip(1).Where(File.Exists));
            if (Model.CheckForUpdates)
                BackgroundUpdateChecker.CheckAppUpdate();
            //{
            //    new Thread(() =>
            //    {
            //        try
            //        {
            //            if (new UpdateChecker().CheckForUpdate(out var updateLog) && !Model.ExecutionInProgress)
            //            {
            //                UpdateCheckDialog.CreateDialog(updateLog).ShowDialog();
            //            }
            //        }
            //        catch
            //        {
            //            // ignored
            //        }
            //    }).Start();
            //}
        }

        private static void FileDropColorOpacityChange(IAnimatable element, double from, double to)
        {
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(0.1),
                EasingFunction = new BackEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };
            element.BeginAnimation(OpacityProperty, animation);
        }

        private void AddConverterTasks(IEnumerable<string> filenames)
        {
            var filenameArray = filenames.ToArray();
            var newFilenames = filenameArray
                .Where(filename => Model.TaskList.All(task => task.ImportPath != filename))
                .ToArray();
            if (string.IsNullOrWhiteSpace(Model.ExportPath.PathValue) && !Model.TaskList.Any() && filenameArray.Any())
            {
                Model.ExportPath.PathValue = Path.GetDirectoryName(filenameArray.First());
            }

            if (Model.AutoDetectFormat)
            {
                var extensions = newFilenames.Select(Path.GetExtension).ToHashSet();
                var matchingExtensions = new HashSet<string>();
                var index = -1;
                foreach (var extension in extensions)
                {
                    for (var i = 0; i < Model.Plugins.Count; ++i)
                    {
                        if (extension != "." + Model.Plugins[i].Suffix)
                        {
                            continue;
                        }
                        matchingExtensions.Add(extension);
                        index = i;
                        break;
                    }
                }
                if (matchingExtensions.Count == 1)
                {
                    Model.SelectedInputPluginIndex = index;
                }
            }

            foreach (var filename in newFilenames)
            {
                Model.TaskList.Add(new TaskViewModel
                {
                    ImportPath = filename,
                    ExportTitle = Path.GetFileNameWithoutExtension(filename),
                    Status = TaskStates.Ready
                });
            }
        }

        private void FilterTasks(Predicate<TaskViewModel> filter)
        {
            var i = 0;
            while (i < Model.TaskList.Count)
            {
                if (!filter(Model.TaskList[i]))
                {
                    Model.TaskList.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }

        private void ExecuteTasks()
        {
            if (!Model.TaskList.Any())
            {
                return;
            }

            // Prepare for execution
            new Thread(() =>
            {
                Model.ExecutionInProgress = true;
                foreach (var task in Model.TaskList)
                {
                    task.PrepareForExecution();
                }
                
                // Construct options
                var inputOptionDictionary = new Dictionary<string, string>();
                foreach (var option in Model.SelectedInputOptions)
                {
                    inputOptionDictionary[option.OptionInfo.Name] = option.OptionValue;
                }
                var outputOptionDictionary = new Dictionary<string, string>();
                foreach (var option in Model.SelectedOutputOptions)
                {
                    outputOptionDictionary[option.OptionInfo.Name] = option.OptionValue;
                }
                var inputOptions = new ConverterOptions(inputOptionDictionary);
                var outputOptions = new ConverterOptions(outputOptionDictionary);
                
                // Common variables
                _skipSameFilename = Model.OverWriteOption == OverwriteOptions.Skip;
                _askBeforeOverwrite = Model.OverWriteOption == OverwriteOptions.Ask;
                
                // Determine number of threads
                // ReSharper disable once InconsistentlySynchronizedField
                _taskQueue = new Queue<TaskViewModel>(Model.TaskList);
                // ReSharper disable once InconsistentlySynchronizedField
                var threadNum = Model.EnableMultiThreading
                    ? Math.Max(1, Math.Min(Environment.ProcessorCount - 2, _taskQueue.Count / 2))
                    : 1;
                
                for (var i = 0; i < threadNum; ++i)
                {
                    _converterThreads.Add(new Thread(() =>
                    {
                        // Create sandbox for tasks
                        var domain = AppDomain.CreateDomain("TaskExecution" + new Random().Next());
                        var container = (TaskContainer)domain.CreateInstanceAndUnwrap(
                            Assembly.GetAssembly(typeof(TaskContainer)).FullName,
                            typeof(TaskContainer).ToString());
                        container.Init(
                            Model.SelectedInputPlugin,
                            Model.SelectedOutputPlugin,
                            inputOptions,
                            outputOptions);
                        _appDomains.Add(domain);

                        while (true)
                        {
                            TaskViewModel task;
                            lock (_taskQueue)
                            {
                                if (!_taskQueue.Any())
                                {
                                    return;
                                }
                                task = _taskQueue.Dequeue();
                            }
                            
                            try
                            {
                                task.ExportDirectory = Model.ExportPath.IsRelative ? Path.Combine(task.ImportDirectory, Model.ExportPath.ActualValue) : Model.ExportPath.PathValue;
                                Directory.CreateDirectory(task.ExportDirectory);
                                task.ExportPath = Path.Combine(task.ExportDirectory, task.ExportTitle + Model.ExportExtension);
                                if (File.Exists(task.ExportPath))
                                {
                                    lock (_overwriteDialogLock)
                                    {
                                        if (_askBeforeOverwrite)
                                        {
                                            var dialog = FileOverwriteDialog.CreateDialog(task.ExportPath);
                                            dialog.ShowDialog();
                                            _skipSameFilename = !dialog.Overwrite;
                                            _askBeforeOverwrite = !dialog.KeepChoice;
                                        }
                                        if (_skipSameFilename)
                                        {
                                            task.Status = TaskStates.Skipped;
                                            continue;
                                        }
                                    }
                                }
                                // Run the container
                                container.Run(task.ImportPath, task.ExportPath);
                            }
                            catch (Exception e)
                            {
                                task.Status = TaskStates.Error;
                                task.Error = e.Message;
                                continue;
                            }
                            var warnings = container.GetWarnings();
                            if (warnings.Any())
                            {
                                task.Status = TaskStates.Warning;
                                foreach (var warning in warnings)
                                {
                                    task.Warnings.Add(warning);
                                }
                                container.ClearWarnings();
                            }
                            else
                            {
                                task.Status = TaskStates.Success;
                            }
                        }
                    }));
                }
                lock (_converterThreads)
                {
                    _converterThreads.ForEach(t => t.Start());
                    _converterThreads.ForEach(t => t.Join());
                    _converterThreads.Clear();
                    _taskQueue = null;
                    
                    // Things after execution
                    Model.ExecutionInProgress = false;
                    Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
                    if (Model.OpenExportFolder)
                    {
                        foreach (var folder in Model.TaskList.Select(task => task.ExportDirectory).ToHashSet())
                        {
                            Process.Start("explorer.exe", folder);
                        }
                    }
                    // Unload the domain to release assembly files
                    _appDomains.ForEach(AppDomain.Unload);
                    _appDomains.Clear();
                }
            }).Start();
        }

        private void QuitApplication()
        {
            lock (_converterThreads)
            {
                _converterThreads.ForEach(t =>
                {
                    try
                    {
                        t.Abort();
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }
            new AppConfig
            {
                Properties =
                {
                    MainWindowSize = new Tuple<double, double>(Width, Height),
                    MainWindowState = WindowState
                },
                Settings =
                {
                    ImportPluginId = Model.SelectedInputPlugin?.Identifier,
                    ExportPluginId = Model.SelectedOutputPlugin?.Identifier,
                    AutoDetectFormat = Model.AutoDetectFormat,
                    AutoResetTasks = Model.AutoResetTasks,
                    AutoExtension = Model.AutoExtension,
                    OpenExportFolder = Model.OpenExportFolder,
                    OverwriteOption = Model.OverWriteOption,
                    DefaultExportPath = Model.DefaultExportPath,
                    CustomExportPaths = Model.CustomExportPaths.Select(path => new PathConfig
                    {
                        Path = path.PathValue,
                        Selected = path == Model.SelectedCustomExportPath
                    }).ToArray(),
                    LastExportPath = Model.DefaultExportPath == ExportPaths.Unset && !string.IsNullOrWhiteSpace(Model.ExportPath.PathValue)
                        ? Model.ExportPath.PathValue
                        : null,
                    EnableMultiThreading = Model.EnableMultiThreading,
                    AppearanceTheme = Model.AppearanceThemes,
                    CheckForUpdates = Model.CheckForUpdates
                }
            }.SaveToFile();
            ToastNotificationManagerCompat.Uninstall();//Uninstall toast notification.
            // Shutdown the application by force
            new Thread(() =>
            {
                Environment.Exit(0);
            }).Start();
        }

        public static readonly RelayCommand<MainWindow> ImportCommand = new RelayCommand<MainWindow>(
            p => !p.Model.ExecutionInProgress,
            p => p.FileMaskPanel_Click(null, null));

        public static readonly RelayCommand<MainWindow> ExportCommand = new RelayCommand<MainWindow>(
            p => p.StartExecutionButton.IsEnabled,
            p => p.StartExecutionButton_Click(null, null));

        public static readonly RelayCommand<MainWindow> BrowseAndExportCommand = new RelayCommand<MainWindow>(
            p => p.StartExecutionButton.IsEnabled,
            p => p.BrowseAndExportMenu_Click(null, null));

        public static readonly RelayCommand<AppModel> ResetCommand = new RelayCommand<AppModel>(
            p => !p.ExecutionInProgress,
            p => p.TaskList.Clear());

        public static readonly RelayCommand<object> CheckUpdateCommand = new RelayCommand<object>(
            p => true,
            p =>
            {
                UpdateCheckDialog.CreateDialog().ShowDialog();
            });

        public static readonly RelayCommand<AppModel> AboutCommand = new RelayCommand<AppModel>(
            p => true,
            p =>
            {
                var dialog = AboutDialog.CreateDialog();
                dialog.DataContext = p;
                dialog.ShowDialog();
            });

        public static readonly RelayCommand<MenuItem> ImportPluginMenuItemCommand = new RelayCommand<MenuItem>(
            p =>
            {
                var model = ((MainWindow)Application.Current.MainWindow)?.Model;
                return model != null && !model.ExecutionInProgress;
            },
            p =>
            {
                if (!p.IsChecked)
                {
                    // Cancelling the choice of plugin is not allowed
                    return;
                }
                var model = ((MainWindow)Application.Current.MainWindow)?.Model;
                if (model == null)
                {
                    return;
                }
                var index = model.Plugins.IndexOf((Plugin)p.DataContext);
                model.SelectedInputPluginIndex = index;
            });

        public static readonly RelayCommand<MenuItem> ExportPluginMenuItemCommand = new RelayCommand<MenuItem>(
            p =>
            {
                var model = ((MainWindow)Application.Current.MainWindow)?.Model;
                return model != null && !model.ExecutionInProgress;
            },
            p =>
            {
                if (!p.IsChecked)
                {
                    // Cancelling the choice of plugin is not allowed
                    return;
                }
                var model = ((MainWindow)Application.Current.MainWindow)?.Model;
                if (model == null)
                {
                    return;
                }
                var index = model.Plugins.IndexOf((Plugin)p.DataContext);
                model.SelectedOutputPluginIndex = index;
            });

        public static readonly RelayCommand<AppModel> SwapPluginCommand = new RelayCommand<AppModel>(
            p => p != null && !p.ExecutionInProgress && p.SelectedInputPluginIndex != p.SelectedOutputPluginIndex,
            p =>
            {
                (p.SelectedInputPluginIndex, p.SelectedOutputPluginIndex) = (p.SelectedOutputPluginIndex, p.SelectedInputPluginIndex);
            });

        public static readonly RelayCommand<AppModel> InstallPluginCommand = new RelayCommand<AppModel>(
            p => p != null && !p.ExecutionInProgress,
            p =>
            {
                var dialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Title = "安装插件",
                    Filter = "插件压缩包 (*.zip)|*.zip"
                };
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                new Thread(() =>
                {
                    var installer = new PluginInstaller();
                    installer.Install(dialog.FileNames);
                }).Start();
            });

        public static readonly RelayCommand<AppModel> ManagePathsCommand = new RelayCommand<AppModel>(
            p => !p.ExecutionInProgress,
            p => PathManagerDialog.CreateDialog(p).ShowDialog());

        private void FileMaskPanel_Focus(object sender, RoutedEventArgs e)
        {
            FileDropColorOpacityChange(FileDropIcon, 0.4, 0.8);
        }

        private void FileMaskPanel_UnFocus(object sender, RoutedEventArgs e)
        {
            FileDropColorOpacityChange(FileDropIcon, 0.8, 0.4);
        }

        private void VisitMarketMaskPanel_Focus(object sender, RoutedEventArgs e)
        {
            FileDropColorOpacityChange(VisitMarketPluginIcon, 0.4, 0.8);
        }

        private void VisitMarketMaskPanel_UnFocus(object sender, RoutedEventArgs e)
        {
            FileDropColorOpacityChange(VisitMarketPluginIcon, 0.8, 0.4);
        }

        private void InstallPluginMaskPanel_Focus(object sender, RoutedEventArgs e)
        {
            FileDropColorOpacityChange(InstallPluginIcon, 0.4, 0.8);
        }

        private void InstallPluginMaskPanel_UnFocus(object sender, RoutedEventArgs e)
        {
            FileDropColorOpacityChange(InstallPluginIcon, 0.8, 0.4);
        }

        private void FileMaskPanel_Click(object sender, RoutedEventArgs e)
        {
            if (e is MouseButtonEventArgs mbe && mbe.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            string FilterOfPlugin(Plugin plugin)
            {
                return $"{plugin.Format} (*.{plugin.Suffix})|*.{plugin.Suffix}";
            }

            var filters = new List<string>();
            if (Model.SelectedInputPluginIndex < 0)
            {
                filters.Add("所有文件 (*.*)|*");
                filters.AddRange(Model.Plugins.Select(FilterOfPlugin));
            }
            else
            {
                filters.Add(FilterOfPlugin(Model.SelectedInputPlugin));
                filters.Add("所有文件 (*.*)|*");
            }


            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "导入工程文件",
                Filter = string.Join("|", filters.ToArray())
            };
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            AddConverterTasks(dialog.FileNames);
        }

        private void FileMaskPanel_Drop(object sender, DragEventArgs e)
        {
            AddConverterTasks((string[])e.Data.GetData(DataFormats.FileDrop));
        }

        private void ImportPluginComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Model.AutoResetTasks && Model.TaskList.Any())
            {
                var plugin = Model.SelectedInputPlugin;
                if (plugin == null)
                {
                    return;
                }
                FilterTasks(task => Path.GetExtension(task.ImportFilename) == "." + plugin.Suffix);
            }
            foreach (var task in Model.TaskList)
            {
                task.Initialize();
            }
        }

        private void ExportPluginComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var task in Model.TaskList)
            {
                task.Initialize();
            }
        }

        private void ClearTasksButton_Click(object sender, RoutedEventArgs e)
        {
            Model.TaskList.Clear();
        }

        private void RestoreTasksButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var task in Model.TaskList)
            {
                task.ExportTitle = Path.GetFileNameWithoutExtension(task.ImportPath);
            }
        }

        private void FilterTasksButton_Click(object sender, RoutedEventArgs e)
        {
            var plugin = Model.SelectedInputPlugin;
            if (plugin == null)
            {
                return;
            }
            FilterTasks(task => Path.GetExtension(task.ImportFilename) == "." + plugin.Suffix);
        }

        private void TreeViewHeader_Click(object sender, RoutedEventArgs e)
        {
            var treeViewItem = ElementsHelper.FindParent<TreeViewItem>(sender as DependencyObject);
            treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
        }

        private void OptionScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var eventArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = e.Source
            };

            var scv = (ScrollViewer)sender;
            scv.RaiseEvent(eventArgs);
            e.Handled = true;
        }

        private void OptionScrollViewer_TouchMove(object sender, TouchEventArgs e)
        {
            var eventArgs = new TouchEventArgs(e.TouchDevice, e.Timestamp)
            {
                RoutedEvent = TouchMoveEvent,
                Source = e.Source
            };

            var scv = (ScrollViewer)sender;
            scv.RaiseEvent(eventArgs);
            e.Handled = true;
        }

        private void OptionTreeView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            OptionScrollViewer_MouseWheel(OptionScrollViewer, e);
        }

        private void OptionTreeView_TouchMove(object sender, TouchEventArgs e)
        {
            OptionScrollViewer_TouchMove(OptionScrollViewer, e);
        }

        private void BrowseExportFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = "选择输出路径",
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }
            Model.ExportPath.PathValue = dialog.FileName;
        }

        private void ExportPathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
                ElementsHelper.FindParent<Card>((TextBox)sender).Focus();
            }
        }

        private void StartExecutionButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Model.ExportPath.PathValue) && Model.DefaultExportPath != ExportPaths.Source)
            {
                BrowseExportFolderButton_Click(sender, e);
                if (string.IsNullOrWhiteSpace(Model.ExportPath.PathValue))
                {
                    return;
                }
            }
            ExecuteTasks();
        }

        private void BrowseAndExportMenu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = "选择输出路径",
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }
            Model.ExportPath.PathValue = dialog.FileName;
            if (StartExecutionButton.IsEnabled)
            {
                StartExecutionButton_Click(sender, e);
            }
        }

        private void UserPreference_ThemeChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (Model.AppearanceThemes != AppearanceThemes.System || e.Category != UserPreferenceCategory.General)
            {
                return;
            }
            Model.AppearanceThemes = AppearanceThemes.System;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Model.ExecutionInProgress)
            {
                e.Cancel = true;
                YesNoDialog.CreateDialog(
                    "确认退出？",
                    "转换任务仍在进行中。现在退出可能导致输出文件损坏。",
                    yesAction: () =>
                    {
                        QuitApplication();
                        Close();
                    }).ShowDialog();
            }
            else
            {
                QuitApplication();
            }
        }
    }
}
