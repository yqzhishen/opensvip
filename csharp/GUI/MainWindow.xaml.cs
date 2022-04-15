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
                Duration = TimeSpan.FromSeconds(0.1),
                EasingFunction = new BackEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };
            FileDropIcon.BeginAnimation(PackIcon.OpacityProperty, animation);
        }

        private void AddConverterTasks(IEnumerable<string> filenames)
        {
            var newFilenames = filenames
                .Where(filename => Model.TaskList.All(task => task.ImportPath != filename))
                .ToArray();
            if (!Model.TaskList.Any() && filenames.Any())
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
            if (!Model.TaskList.Any() || Model.SelectedInputPluginIndex < 0 || Model.SelectedOutputPluginIndex < 0)
            {
                return;
            }
            else if (String.IsNullOrWhiteSpace(Model.ExportPath))
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
            BrowseExportFolderButton_Click(sender, e);
            StartExecutionButton_Click(sender, e);
        }

        private void TreeViewHeader_Click(object sender, RoutedEventArgs e)
        {
            var treeViewItem = ElementsHelper.FindParent<TreeViewItem>(sender as DependencyObject);
            treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
        }

        private void OptionScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = e.Source;

            ScrollViewer scv = (ScrollViewer)sender;
            scv.RaiseEvent(eventArg);
            e.Handled = true;
        }

        private void OptionTreeView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            OptionScrollViewer_MouseWheel(OptionScrollViewer, e);
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
                writer.Write(JsonConvert.SerializeObject(Model));
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
