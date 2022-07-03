using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using OpenSvip.Framework;
using OpenSvip.GUI.Config;

namespace OpenSvip.GUI.Dialog
{
    /// <summary>
    /// PluginDownloadDialog.xaml 的交互逻辑
    /// </summary>
    public partial class PluginDownloadDialog : INotifyPropertyChanged
    {
        private UpdateLog _updateLog;

        public UpdateLog UpdateLog
        {
            get => _updateLog;
            set
            {
                _updateLog = value;
                OnPropertyChanged();
            }
        }

        private DownloadStates _status;

        public DownloadStates Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public PluginDownloadDialog(UpdateLog updateLog)
        {
            InitializeComponent();
            UpdateLog = updateLog;
            DataContext = this;
        }

        public static PluginDownloadDialog CreateDialog(UpdateLog updateLog)
        {
            PluginDownloadDialog dialog = null;
            Application.Current.Dispatcher.Invoke(() => { dialog = new PluginDownloadDialog(updateLog); });
            return dialog;
        }

        public void ShowDialog()
        {
            Dispatcher.Invoke(() => { DialogHost.Show(this, "RootDialogHost"); });
        }

        private void DownloadingTemplate_Initialized(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                try
                {
                    var filename =
                        UpdateLog.DownloadLink.Substring(
                            UpdateLog.DownloadLink.LastIndexOf("/", StringComparison.Ordinal) + 1);
                    Directory.CreateDirectory(ConstValues.CommonDownloadPath);
                    var savePath = Path.Combine(ConstValues.CommonDownloadPath, filename);

                    var client = new WebClient();
                    client.DownloadFile(UpdateLog.DownloadLink, savePath);
                    Dispatcher.Invoke(() => { DialogHost.Close("RootDialogHost"); });

                    var installer = new PluginInstaller();
                    installer.Install(savePath);
                    if (installer.Success > 0)
                    {
                        File.Delete(savePath);
                    }
                }
                catch
                {
                    Status = DownloadStates.Failed;
                }
            }).Start();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            Status = DownloadStates.Downloading;
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            Status = DownloadStates.Downloading;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum DownloadStates
    {
        Ready,
        Downloading,
        Failed
    }

    public class PluginDownloadDialogTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ReadyTemplate { get; set; }

        public DataTemplate DownloadingTemplate { get; set; }

        public DataTemplate FailedTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            try // prevent the designing view from throwing exceptions
            {
                if (container == null)
                {
                    return ReadyTemplate;
                }

                var self = ElementsHelper.FindParent<PluginDownloadDialog>(container);
                switch (self.Status)
                {
                    case DownloadStates.Ready:
                        return ReadyTemplate;
                    case DownloadStates.Downloading:
                        return DownloadingTemplate;
                    case DownloadStates.Failed:
                        return FailedTemplate;
                    default:
                        return ReadyTemplate;
                }
            }
            catch (Exception)
            {
                return ReadyTemplate;
            }
        }
    }
}