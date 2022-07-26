using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OpenSvip.GUI.Dialog;

namespace OpenSvip.GUI
{
    /// <summary>
    /// TaskListViewItem.xaml 的交互逻辑
    /// </summary>
    public partial class TaskListViewItem
    {
        public TaskListViewItem()
        {
            InitializeComponent();
        }

        private void ExportTitleTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
            }
        }

        private void RemoveTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var itemsControl = ElementsHelper.FindParent<ItemsControl>(this);
            (itemsControl.ItemsSource as Collection<TaskViewModel>)?.Remove(DataContext as TaskViewModel);
        }

        private void OpenProjectFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(((TaskViewModel)DataContext).ExportPath);
            }
            catch (Exception exception)
            {
                MessageDialog.CreateDialog("打开文件出错", exception.Message).ShowDialog();
            }
        }

        private void OpenTargetFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var task = (TaskViewModel)DataContext;
            if (File.Exists(task.ExportPath))
            {
                Process.Start("explorer.exe", $"/select,{task.ExportPath}");
            }
            else
            {
                Process.Start("explorer.exe", Path.GetDirectoryName(task.ExportPath));
            }
        }

        private void CopyErrorMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var task = (TaskViewModel)DataContext;
            var button = (Button)sender;
            try
            {
                Clipboard.SetText(task.Error);
                button.Content = "已复制";
            }
            catch (Exception)
            {
                button.Content = "复制失败";
            }
            new Thread(() =>
            {
                Thread.Sleep(2000);
                try
                {
                    Dispatcher.Invoke(() => { button.Content = "复制错误信息"; });
                }
                catch
                {
                    // ignored
                }
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
                var task = (TaskViewModel)ElementsHelper.FindParent<TaskListViewItem>(container).DataContext;
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
