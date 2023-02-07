using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using OpenSvip.GUI.Config;

namespace OpenSvip.GUI.Dialog
{
    /// <summary>
    /// PathManagerDialog.xaml 的交互逻辑
    /// </summary>
    public partial class PathManagerDialog
    {
        public AppModel Model => (AppModel)DataContext;

        public PathManagerDialog()
        {
            InitializeComponent();
        }

        private bool _selectionChanged;

        public static PathManagerDialog CreateDialog(AppModel dataContext)
        {
            PathManagerDialog dialog = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                dialog = new PathManagerDialog
                {
                    DataContext = dataContext,
                    _selectionChanged = dataContext.SelectedCustomExportPathIndex < 0
                };
            });
            return dialog;
        }

        public void ShowDialog()
        {
            Dispatcher.Invoke(() =>
            {
                DialogHost.Show(this, "RootDialogHost");
            });
        }

        private void AddPathColorOpacityChange(double from, double to)
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
            AddPathIcon.BeginAnimation(OpacityProperty, animation);
        }

        private void AddPathMaskPanel_Focus(object sender, MouseEventArgs e)
        {
            AddPathColorOpacityChange(0.4, 0.8);
        }

        private void AddPathMaskPanel_UnFocus(object sender, MouseEventArgs e)
        {
            AddPathColorOpacityChange(0.8, 0.4);
        }

        private void AddPathMaskPanel_Click(object sender, RoutedEventArgs e)
        {
            Model.CustomExportPaths.Add(new CustomPath());
            PathScrollViewer.ScrollToEnd();
        }

        private void BrowsePathButton_Click(object sender, RoutedEventArgs e)
        {
            var customPath = (CustomPath)((Button)sender).DataContext;
            var dialog = new CommonOpenFileDialog
            {
                Title = "浏览自定义路径",
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }
            customPath.PathValue = dialog.FileName;
            if (customPath == Model.SelectedCustomExportPath)
            {
                Model.ExportPath.PathValue = dialog.FileName;
            }
        }

        private void RemovePathButton_Click(object sender, RoutedEventArgs e)
        {
            var customPath = (CustomPath)((Button)sender).DataContext;
            if (customPath == Model.SelectedCustomExportPath)
            {
                Model.SelectedCustomExportPathIndex = -1;
            }
            (PathListView.ItemsSource as Collection<CustomPath>)?.Remove(customPath);
        }

        private void SelectPathRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var customPath = (CustomPath)((RadioButton)sender).DataContext;
            var customPaths = (Collection<CustomPath>)PathListView.ItemsSource;
            Model.SelectedCustomExportPathIndex = customPaths.IndexOf(customPath);
            if (_selectionChanged)
            {
                Model.DefaultExportPath = ExportPaths.Custom;
            }
            _selectionChanged = true;
        }

        private void SelectPathRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Model.SelectedCustomExportPathIndex = -1;
        }

        private void PathValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
                ElementsHelper.FindParent<ScrollViewer>((TextBox)sender).Focus();
            }
        }

        private void PathValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if ((CustomPath)(textBox).DataContext == Model.SelectedCustomExportPath)
            {
                Model.ExportPath.PathValue = textBox.Text;
            }
        }

        private void PathValueTextBox_TextCleared(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (string.IsNullOrEmpty(textBox.Text) && (CustomPath)(textBox).DataContext == Model.SelectedCustomExportPath)
            {
                Model.ExportPath.PathValue = "";
            }
        }
    }
}
