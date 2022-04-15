using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace OpenSvip.GUI
{
    /// <summary>
    /// TaskListViewItem.xaml 的交互逻辑
    /// </summary>
    public partial class TaskListViewItem : UserControl
    {
        public TaskListViewItem()
        {
            InitializeComponent();
        }

        private void RemoveTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var itemsControl = ElementsHelper.FindParent<ItemsControl>(this);
            (itemsControl.ItemsSource as Collection<TaskViewModel>).Remove(DataContext as TaskViewModel);
        }

        private void OpenProjectFileButton_Click(object sender, RoutedEventArgs e)
        {
            var model = ElementsHelper.FindParent<Window>(this).DataContext as AppModel;
            var task = DataContext as TaskViewModel;
            Process.Start(Path.Combine(model.ExportPath, task.ExportTitle + model.ExportExtension));
        }

        private void OpenTargetFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var model = ElementsHelper.FindParent<Window>(this).DataContext as AppModel;
            var task = DataContext as TaskViewModel;
            Process.Start("explorer.exe", $"/select,{Path.Combine(task.ExportFolder, task.ExportTitle + model.ExportExtension)}");
        }

        private void CopyErrorMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var task = DataContext as TaskViewModel;
            Clipboard.SetText(task.Error);
            var button = sender as Button;
            button.Content = "已复制";
            new Thread(() =>
            {
                Thread.Sleep(5000);
                Dispatcher.Invoke(() =>
                {
                    button.Content = "复制错误信息";
                });
            }).Start();
        }
    }

    public class TaskListViewItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ReadyTemplate { get; set; }

        public DataTemplate QueuedTemplate { get; set; }

        public DataTemplate SuccessTemplate { get; set; }

        public DataTemplate WarningTemplate { get; set; }

        public DataTemplate ErrorTemplate { get; set; }

        public DataTemplate SkippedTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            try // prevent the designing view from throwing exceptions
            {
                if (container == null)
                {
                    return ReadyTemplate;
                }
                var task = ElementsHelper.FindParent<TaskListViewItem>(container).DataContext as TaskViewModel;
                switch (task.Status)
                {
                    case TaskStates.Ready:
                        return ReadyTemplate;
                    case TaskStates.Queued:
                        return QueuedTemplate;
                    case TaskStates.Success:
                        return SuccessTemplate;
                    case TaskStates.Warning:
                        return WarningTemplate;
                    case TaskStates.Error:
                        return ErrorTemplate;
                    case TaskStates.Skipped:
                        return SkippedTemplate;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
