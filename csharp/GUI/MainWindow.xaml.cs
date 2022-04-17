using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using OpenSvip.Framework;
using System.Threading;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Input;
using System.Reflection;
using Newtonsoft.Json;

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
            try
            {
                FileStream stream = new FileStream(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Configurations.json"),
                    FileMode.Open,
                    FileAccess.Read);
                StreamReader reader = new StreamReader(stream);
                Model = JsonConvert.DeserializeObject<AppModel>(reader.ReadToEnd());
                reader.Close();
                stream.Close();
            }
            catch (Exception)
            {
                Model = new AppModel();
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
            if (Model.DefaultExportPath == DefaultExport.None && !Model.TaskList.Any() && filenames.Any())
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
                        if (extension == "." + Model.Plugins[i].Suffix)
                        {
                            matchingExtensions.Add(extension);
                            index = i;
                            break;
                        }
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
                foreach (var task in Model.TaskList)
                {
                    task.ExportFolder = Model.DefaultExportPath == DefaultExport.Source && string.IsNullOrWhiteSpace(Model.ExportPath) ? task.ImportDirectory : Model.ExportPath;
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
                if (Model.OpenExportFolder)
                {
                    var openFolder = Model.ExportPath;
                    if (Model.DefaultExportPath == DefaultExport.Source)
                    {
                        openFolder = Model.TaskList[0].ImportDirectory;
                        foreach (var task in Model.TaskList.Skip(1))
                        {
                            if (task.ImportDirectory != openFolder)
                            {
                                return;
                            }
                        }
                    }
                    Process.Start("explorer.exe", openFolder);
                }
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
            if (String.IsNullOrWhiteSpace(Model.ExportPath) && Model.DefaultExportPath != DefaultExport.Source)
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
            try
            {
                FileStream stream = new FileStream(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Configurations.json"),
                    FileMode.Create,
                    FileAccess.Write);
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(JsonConvert.SerializeObject(Model, Formatting.Indented));
                writer.Flush();
                stream.Flush();
                writer.Close();
                stream.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
