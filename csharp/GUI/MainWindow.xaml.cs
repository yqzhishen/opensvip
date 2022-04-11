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

namespace OpenSvip.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public AppModel Model { get; set; } = AppModel.Model;

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
            var newFilenames = filenames.Where(filename => !Model.TaskList.Any(task => task.ImportPath == filename));

            if (Model.AutoDetectFormat)
            {
                var extensions = newFilenames.Select(filename => Path.GetExtension(filename)).ToHashSet();
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
                var exportFilename = Model.SelectedOutputPluginIndex >= 0
                    ? Path.GetFileNameWithoutExtension(filename) + "." + Model.Plugins[Model.SelectedOutputPluginIndex].Suffix
                    : Path.GetFileName(filename);
                Model.TaskList.Add(new Task
                {
                    ImportPath = filename,
                    ExportFilename = exportFilename,
                    Status = "ready"
                });
            }
        }

        private void ExecuteTasks()
        {
            new Thread(() =>
            {
                Model.ExecutionInProgress = true;
                double progressStep = 100.0 / Model.TaskList.Count;

                var inputConverter = PluginManager.GetConverter(Model.Plugins[Model.SelectedInputPluginIndex].Identifier);
                var outputConverter = PluginManager.GetConverter(Model.Plugins[Model.SelectedOutputPluginIndex].Identifier);
                foreach (var task in Model.TaskList)
                {
                    outputConverter.Save(
                        Path.Combine(Model.ExportPath, task.ExportFilename),
                        inputConverter.Load(
                            task.ImportPath,
                            new ConverterOptions(new Dictionary<string, string>())),
                        new ConverterOptions(new Dictionary<string, string>()));
                }
                Model.ExecutionInProgress = false;
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
                filters.Add(FilterOfPlugin(Model.Plugins[Model.SelectedInputPluginIndex]));
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
                Model.TaskList.Clear();
            }
        }

        private void RemoveTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var grid = ElementsHelper.FindParent<Grid>(sender as System.Windows.Controls.Button);
            Model.TaskList.Remove(grid.DataContext as Task);
        }

        private void ClearTaskButton_Click(object sender, RoutedEventArgs e)
        {
            Model.TaskList.Clear();
        }

        private void BrowseExportFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            Model.ExportPath = dialog.SelectedPath;
        }

        private void StartExecutionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Model.TaskList.Any() || Model.SelectedInputPluginIndex < 0 || Model.SelectedOutputPluginIndex < 0)
            {
                return;
            }
            ExecuteTasks();
        }
    }
}
