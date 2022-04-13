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

namespace OpenSvip.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public AppModel Model { get; set; } = new AppModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = Model;
            foreach (var str in Model.Formats)
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
            TaskListView.ItemsSource = Model.TaskList;
        }

        private void FileDropColorOpacityChange(double from, double to)
        {
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            FileDropIcon.BeginAnimation(PackIcon.OpacityProperty, animation);
        }

        private void AddConverterTasks(IEnumerable<string> filenames)
        {
            var newFilenames = filenames
                .Where(filename => Model.TaskList.All(task => task.ImportPath != filename))
                .ToArray();

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
                Model.TaskList.Add(new Task
                {
                    ImportPath = filename,
                    ExportTitle = Path.GetFileNameWithoutExtension(filename),
                    Status = TaskStates.Ready
                });
            }
        }

        private void FilterTasks(Predicate<Task> filter)
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
                    var exportPath = Path.Combine(Model.ExportPath, task.ExportTitle + Model.ExportExtension);
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
                        outputConverter.Save(
                            exportPath,
                            inputConverter.Load(
                                task.ImportPath,
                                new ConverterOptions(new Dictionary<string, string>())),
                            new ConverterOptions(new Dictionary<string, string>()));
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
                    Process.Start("explorer.exe", Model.ExportPath);
                }
            }).Start();

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
            if (!Model.TaskList.Any() || Model.SelectedInputPluginIndex < 0 || Model.SelectedOutputPluginIndex < 0 || String.IsNullOrWhiteSpace(Model.ExportPath))
            {
                return;
            }
            ExecuteTasks();
        }
    }
}
