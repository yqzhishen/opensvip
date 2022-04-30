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
using System.Reflection;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Input;
using Microsoft.Win32;
using OpenSvip.GUI.Config;
using OpenSvip.GUI.Dialog;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace OpenSvip.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public AppModel Model { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            SystemEvents.UserPreferenceChanged += UserPreference_ThemeChanged;

            var config = AppConfig.LoadFromFile();

            var properties = config.Properties;
            var bounds = properties.MainRestoreBounds;
            if (bounds.Width > 0 && bounds.Height > 0)
            {
                WindowState = WindowState.Normal;
                Left = bounds.Left;
                Top = bounds.Top;
                Width = bounds.Width;
                Height = bounds.Height;
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
                AppearanceThemes = settings.AppearanceTheme
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
            
            var formats = Model.Formats;
            foreach (var str in formats)
            {
                ImportPluginComboBox.Items.Add(new ComboBoxItem
                {
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                    Content = str
                });
                ExportPluginComboBox.Items.Add(new ComboBoxItem
                {
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                    Content = str
                });
            }
            for (var i = 0; i < formats.Count; ++i)
            {
                var importMenuItem = new System.Windows.Controls.MenuItem
                {
                    Header = $"_{(i + 1) % 10}  {formats[i]}"
                };
                importMenuItem.CommandParameter = importMenuItem;
                importMenuItem.Command = ImportPluginMenuItemCommand;
                ImportPluginMenuItem.Items.Add(importMenuItem);
                var exportMenuItem = new System.Windows.Controls.MenuItem
                {
                    Header = $"_{(i + 1) % 10}  {formats[i]}"
                };
                exportMenuItem.CommandParameter = exportMenuItem;
                exportMenuItem.Command = ExportPluginMenuItemCommand;
                ExportPluginMenuItem.Items.Add(exportMenuItem);
            }
            TaskListView.ItemsSource = Model.TaskList;
            AddConverterTasks(Environment.GetCommandLineArgs().Skip(1).Where(File.Exists));
        }

        private void FileDropColorOpacityChange(double from, double to)
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
            FileDropIcon.BeginAnimation(OpacityProperty, animation);
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
            new Thread(() =>
            {
                // Prepare for execution
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
                
                // Create sandbox for tasks
                var domain = AppDomain.CreateDomain("TaskExecution");
                var container = (TaskContainer)domain.CreateInstanceAndUnwrap(
                    Assembly.GetAssembly(typeof(TaskContainer)).FullName,
                    typeof(TaskContainer).ToString());
                container.Init(
                    Model.SelectedInputPlugin,
                    Model.SelectedOutputPlugin,
                    inputOptions,
                    outputOptions);
                
                var skipSameFilename = Model.OverWriteOption == OverwriteOptions.Skip;
                var askBeforeOverwrite = Model.OverWriteOption == OverwriteOptions.Ask;
                foreach (var task in Model.TaskList)
                {
                    try
                    {
                        task.ExportDirectory = Model.ExportPath.IsRelative ? Path.Combine(task.ImportDirectory, Model.ExportPath.ActualValue) : Model.ExportPath.PathValue;
                        Directory.CreateDirectory(task.ExportDirectory);
                        task.ExportPath = Path.Combine(task.ExportDirectory, task.ExportTitle + Model.ExportExtension);
                        if (File.Exists(task.ExportPath))
                        {
                            if (askBeforeOverwrite)
                            {
                                var dialog = FileOverwriteDialog.CreateDialog(task.ExportPath);
                                dialog.ShowDialog();
                                skipSameFilename = !dialog.Overwrite;
                                askBeforeOverwrite = !dialog.KeepChoice;
                            }
                            if (skipSameFilename)
                            {
                                task.Status = TaskStates.Skipped;
                                continue;
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
                // Unload the domain to release assembly files
                AppDomain.Unload(domain);
                
                // Things after execution
                Model.ExecutionInProgress = false;
                if (!Model.OpenExportFolder)
                {
                    return;
                }
                foreach (var folder in Model.TaskList.Select(task => task.ExportDirectory).ToHashSet())
                {
                    Process.Start("explorer.exe", folder);
                }
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

        public static readonly RelayCommand<MainWindow> AboutCommand = new RelayCommand<MainWindow>(
            p => true,
            p => p.AboutMenuItem_Click(null, null));

        public static readonly RelayCommand<System.Windows.Controls.MenuItem> ImportPluginMenuItemCommand = new RelayCommand<System.Windows.Controls.MenuItem>(
            p =>
            {
                var model = (AppModel)p.DataContext;
                return model != null && !model.ExecutionInProgress;
            },
            p =>
            {
                if (!p.IsChecked)
                {
                    // Cancelling the choice of plugin is not allowed
                    return;
                }
                var parent = p.Parent;
                var index = ((ItemsControl)parent).Items.IndexOf(p);
                ((MainWindow)App.Current.MainWindow).Model.SelectedInputPluginIndex = index;
            });

        public static readonly RelayCommand<System.Windows.Controls.MenuItem> ExportPluginMenuItemCommand = new RelayCommand<System.Windows.Controls.MenuItem>(
            p => true,
            p =>
            {
                if (!p.IsChecked)
                {
                    // Cancelling the choice of plugin is not allowed
                    return;
                }
                var parent = p.Parent;
                var index = ((ItemsControl)parent).Items.IndexOf(p);
                ((MainWindow)App.Current.MainWindow).Model.SelectedOutputPluginIndex = index;
            });

        public static readonly RelayCommand<AppModel> InstallPluginCommand = new RelayCommand<AppModel>(
            p => !p.ExecutionInProgress,
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
                    var success = 0;
                    foreach (var path in dialog.FileNames)
                    {
                        try
                        {
                            var plugin = PluginManager.ExtractPlugin(path, out var folder);
                            YesNoDialog confirmDialog;
                            if (!PluginManager.HasPlugin(plugin.Identifier))
                            {
                                confirmDialog = YesNoDialog.CreateDialog(
                                    "安装新的插件",
                                    $"将要安装由 {plugin.Author} 开发，适用于 {plugin.Format} (*.{plugin.Suffix}) 的插件“{plugin.Name}”。确认继续？",
                                    "安装");
                                if (confirmDialog.ShowDialog())
                                {
                                    PluginManager.InstallPlugin(plugin, folder);
                                    ++success;
                                }
                                continue;
                            }
                            var oldPlugin = PluginManager.GetPlugin(plugin.Identifier);
                            var oldVersion = new Version(oldPlugin.Version);
                            var version = new Version(plugin.Version);
                            if (version > oldVersion)
                            {
                                confirmDialog = YesNoDialog.CreateDialog(
                                    "更新已有插件",
                                    $"插件“{plugin.Name}”将由 {oldVersion} 更新至 {version}。确认继续？",
                                    "更新");
                            }
                            else
                            {
                                confirmDialog = YesNoDialog.CreateDialog(
                                    "此插件不是新的版本",
                                    $"当前已安装插件“{plugin.Name}”的相同或更新版本 ({oldVersion} ≥ {version})。确认要覆盖安装吗？",
                                    "覆盖");
                            }
                            if (confirmDialog.ShowDialog())
                            {
                                PluginManager.InstallPlugin(plugin, folder);
                                ++success;
                            }
                        }
                        catch (Exception e)
                        {
                            MessageDialog.CreateDialog("插件安装失败", e.Message).ShowDialog();
                        }
                    }
                    if (success > 0)
                    {
                        var restartDialog = YesNoDialog.CreateDialog(
                            "插件安装完成。重启本应用？",
                            $"已成功安装 {success} 个插件。需要重启本应用以使新的功能生效。",
                            "重启",
                            "稍后");
                        if (restartDialog.ShowDialog())
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                System.Windows.Application.Current.MainWindow?.Close();
                                System.Windows.Application.Current.Shutdown();
                            });
                            System.Windows.Forms.Application.Restart();
                        }
                    }
                    if (Directory.Exists(PluginManager.TempPath))
                    {
                        new DirectoryInfo(PluginManager.TempPath).Delete(true);
                    }
                }).Start();
            });

        public static readonly RelayCommand<AppModel> ManagePathsCommand = new RelayCommand<AppModel>(
            p => !p.ExecutionInProgress,
            p => PathManagerDialog.CreateDialog(p).ShowDialog());

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = AboutDialog.CreateDialog();
            dialog.DataContext = Model;
            dialog.ShowDialog();
        }

        private void FileMaskPanel_Focus(object sender, RoutedEventArgs e)
        {
            FileDropColorOpacityChange(0.4, 0.8);
        }

        private void FileMaskPanel_UnFocus(object sender, RoutedEventArgs e)
        {
            FileDropColorOpacityChange(0.8, 0.4);
        }

        private void FileMaskPanel_Click(object sender, RoutedEventArgs e)
        {
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

        private void FileMaskPanel_Drop(object sender, System.Windows.DragEventArgs e)
        {
            AddConverterTasks((string[])e.Data.GetData(System.Windows.DataFormats.FileDrop));
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
                RoutedEvent = UIElement.MouseWheelEvent,
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
                RoutedEvent = UIElement.TouchMoveEvent,
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
            new AppConfig
            {
                Properties =
                {
                    MainRestoreBounds = RestoreBounds,
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
                    AppearanceTheme = Model.AppearanceThemes
                }
            }.SaveToFile();
        }
    }
}
