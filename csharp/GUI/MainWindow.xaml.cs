using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using OpenSvip.Framework;
using System.Threading;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Input;
using OpenSvip.GUI.Config;

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
                DefaultExportPath = settings.DefaultExportPath
            };
            Model.SelectedInputPluginIndex = settings.ImportPluginId == null ? -1 : Model.Plugins.FindIndex(plugin => plugin.Identifier.Equals(settings.ImportPluginId));
            Model.SelectedOutputPluginIndex = settings.ExportPluginId == null ? -1 : Model.Plugins.FindIndex(plugin => plugin.Identifier.Equals(settings.ExportPluginId));
            foreach (var path in settings.CustomExportPaths)
            {
                Model.CustomExportPaths.Add(path);
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
            AddConverterTasks(Environment.GetCommandLineArgs().Skip(1).Where(arg => File.Exists(arg)));
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
            var newFilenames = filenames
                .Where(filename => Model.TaskList.All(task => task.ImportPath != filename))
                .ToArray();
            if (Model.DefaultExportPath == ExportPaths.Unset && !Model.TaskList.Any() && filenames.Any())
            {
                Model.ExportPath = Path.GetDirectoryName(filenames.First());
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
                Model.ExecutionInProgress = true;
                var inputConverter = PluginManager.GetConverter(Model.SelectedInputPlugin.Identifier);
                var outputConverter = PluginManager.GetConverter(Model.SelectedOutputPlugin.Identifier);
                foreach (var task in Model.TaskList)
                {
                    task.PrepareForExecution();
                }
                var skipSameFilename = Model.OverWriteOption == OverwriteOptions.Skip;
                var askBeforeOverwrite = Model.OverWriteOption == OverwriteOptions.Ask;
                if (!string.IsNullOrWhiteSpace(Model.ExportPath))
                {
                    Directory.CreateDirectory(Model.ExportPath);
                }
                foreach (var task in Model.TaskList)
                {
                    task.ExportFolder = Model.DefaultExportPath == ExportPaths.Source && string.IsNullOrWhiteSpace(Model.ExportPath) ? task.ImportDirectory : Model.ExportPath;
                    var exportPath = Path.Combine(task.ExportFolder, task.ExportTitle + Model.ExportExtension);
                    if (File.Exists(exportPath))
                    {
                        if (askBeforeOverwrite)
                        {
                            var dialog = FileOverwriteDialog.CreateDialog(exportPath);
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
                    try
                    {
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
                        outputConverter.Save(
                            exportPath,
                            inputConverter.Load(
                                task.ImportPath,
                                new ConverterOptions(inputOptionDictionary)),
                            new ConverterOptions(outputOptionDictionary));
                    }
                    catch (Exception e)
                    {
                        task.Status = TaskStates.Error;
                        task.Error = e.Message;
                        continue;
                    }
                    var warnings = Warnings.GetWarnings();
                    if (warnings.Any())
                    {
                        task.Status = TaskStates.Warning;
                        foreach (var warning in warnings)
                        {
                            task.Warnings.Add(warning);
                        }
                        Warnings.ClearWarnings();
                    }
                    else
                    {
                        task.Status = TaskStates.Success;
                    }
                }
                Model.ExecutionInProgress = false;
                if (!Model.OpenExportFolder)
                {
                    return;
                }
                var openFolder = Model.ExportPath;
                if (Model.DefaultExportPath == ExportPaths.Source)
                {
                    openFolder = Model.TaskList[0].ImportDirectory;
                    if (Model.TaskList.Skip(1).Any(task => task.ImportDirectory != openFolder))
                    {
                        return;
                    }
                }
                Process.Start("explorer.exe", openFolder);
            }).Start();
        }

        public static RelayCommand<MainWindow> ImportCommand = new RelayCommand<MainWindow>(
            p => !p.Model.ExecutionInProgress,
            p => p.FileMaskPanel_Click(null, null));

        public static RelayCommand<MainWindow> ExportCommand = new RelayCommand<MainWindow>(
            p => p.StartExecutionButton.IsEnabled,
            p => p.StartExecutionButton_Click(null, null));

        public static RelayCommand<MainWindow> BrowseAndExportCommand = new RelayCommand<MainWindow>(
            p => p.StartExecutionButton.IsEnabled,
            p => p.BrowseAndExportMenu_Click(null, null));

        public static RelayCommand<AppModel> ResetCommand = new RelayCommand<AppModel>(
            p => !p.ExecutionInProgress,
            p => p.TaskList.Clear());

        public static RelayCommand<MainWindow> AboutCommand = new RelayCommand<MainWindow>(
            p => true,
            p => p.AboutMenuItem_Click(null, null));

        public static RelayCommand<System.Windows.Controls.MenuItem> ImportPluginMenuItemCommand = new RelayCommand<System.Windows.Controls.MenuItem>(
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

        public static RelayCommand<System.Windows.Controls.MenuItem> ExportPluginMenuItemCommand = new RelayCommand<System.Windows.Controls.MenuItem>(
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

        public static RelayCommand<AppModel> InstallPluginCommand = new RelayCommand<AppModel>(
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
                            Version oldVersion = new Version(oldPlugin.Version);
                            Version version = new Version(plugin.Version);
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

            ScrollViewer scv = (ScrollViewer)sender;
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

            ScrollViewer scv = (ScrollViewer)sender;
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
            Model.ExportPath = dialog.FileName;
        }

        private void StartExecutionButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(Model.ExportPath) && Model.DefaultExportPath != ExportPaths.Source)
            {
                BrowseExportFolderButton_Click(sender, e);
                if (String.IsNullOrWhiteSpace(Model.ExportPath))
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
            Model.ExportPath = dialog.FileName;
            if (StartExecutionButton.IsEnabled)
            {
                StartExecutionButton_Click(sender, e);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
                    CustomExportPaths = Model.CustomExportPaths.ToArray()
                }
            }.SaveToFile();
        }
    }
}
