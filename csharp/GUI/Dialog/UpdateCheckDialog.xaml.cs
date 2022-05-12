using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using OpenSvip.GUI.Config;
using Tomlet;

namespace OpenSvip.GUI.Dialog
{
    /// <summary>
    /// UpdateCheckDialog.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateCheckDialog
    {
        public UpdateViewModel Model = new UpdateViewModel();

        private readonly bool _hasChecked;

        public UpdateCheckDialog(bool hasChecked = false)
        {
            InitializeComponent();
            DataContext = Model;
            _hasChecked = hasChecked;
        }

        public static UpdateCheckDialog CreateDialog(UpdateLog updateLog = null)
        {
            UpdateCheckDialog dialog = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                dialog = new UpdateCheckDialog(updateLog != null);
                if (updateLog == null)
                {
                    return;
                }
                dialog.Model.UpdateLog = ConvertFromUpdateLog(updateLog);
                dialog.Model.Status = UpdateStates.Detected;
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

        private static UpdateLogViewModel ConvertFromUpdateLog(UpdateLog updateLog)
        {
            var updateLogViewModel = new UpdateLogViewModel
            {
                NewVersion = updateLog.Version,
                UpdateDate = updateLog.Date,
                Prologue = updateLog.Prologue,
                Epilogue = updateLog.Epilogue,
                DownloadLink = updateLog.DownloadLink
            };
            foreach (var item in updateLog.Items)
            {
                updateLogViewModel.UpdateItems.Add(item);
            }
            return updateLogViewModel;
        }

        private void UpdateCheckingTemplate_Initialized(object sender, EventArgs e)
        {
            if (_hasChecked)
            {
                return;
            }
            new Thread(() =>
            {
                try
                {
                    if (new UpdateChecker().CheckForUpdate(out var updateLog))
                    {
                        Model.UpdateLog = ConvertFromUpdateLog(updateLog);
                        Model.Status = UpdateStates.Detected;
                    }
                    else
                    {
                        Model.Status = UpdateStates.Latest;
                    }
                }
                catch
                {
                    Model.Status = UpdateStates.Failed;
                }
            }).Start();
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            Model.Status = UpdateStates.Checking;
        }
    }

    public class UpdateCheckDialogTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CheckingTemplate { get; set; }

        public DataTemplate LatestTemplate { get; set; }

        public DataTemplate DetectedTemplate { get; set; }

        public DataTemplate FailedTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            try // prevent the designing view from throwing exceptions
            {
                if (container == null)
                {
                    return CheckingTemplate;
                }
                var update = (UpdateViewModel)ElementsHelper.FindParent<UpdateCheckDialog>(container).DataContext;
                switch (update.Status)
                {
                    case UpdateStates.Checking:
                        return CheckingTemplate;
                    case UpdateStates.Latest:
                        return LatestTemplate;
                    case UpdateStates.Detected:
                        return DetectedTemplate;
                    case UpdateStates.Failed:
                        return FailedTemplate;
                    default:
                        return CheckingTemplate;
                }
            }
            catch (Exception)
            {
                return CheckingTemplate;
            }
        }
    }
}
