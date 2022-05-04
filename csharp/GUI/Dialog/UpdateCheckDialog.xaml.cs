using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        public UpdateCheckDialog()
        {
            InitializeComponent();
            DataContext = Model;
        }

        public static UpdateCheckDialog CreateDialog()
        {
            UpdateCheckDialog dialog = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                dialog = new UpdateCheckDialog();
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

        private void UpdateCheckingTemplate_Initialized(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                var timer = new Thread(() =>
                {
                    Thread.Sleep(1000);
                });
                try
                {
                    timer.Start();
                    var request = (HttpWebRequest)WebRequest.Create(Information.UpdateLogUrl);
                    request.Method = "GET";
                    var response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new HttpException($"Unexpected response code: {response.StatusCode}");
                    }
                    var stream = response.GetResponseStream();
                    if (stream == null)
                    {
                        throw new HttpException("Response is null");
                    }
                    using (var reader = new StreamReader(stream))
                    {
                        var responseBody = reader.ReadToEnd();
                        var updateLog = TomletMain.To<UpdateLog>(responseBody);
                        Model.UpdateLog = new UpdateLogViewModel
                        {
                            NewVersion = updateLog.Version,
                            UpdateDate = updateLog.Date,
                            Prologue = updateLog.Prologue,
                            Epilogue = updateLog.Epilogue,
                            DownloadLink = updateLog.DownloadLink
                        };
                        foreach (var item in updateLog.Items)
                        {
                            Model.UpdateLog.UpdateItems.Add(item);
                        }
                    }
                    var currentVersion = new Version(Information.ApplicationVersion.Split(' ')[0]);
                    var newVersion = new Version(Model.UpdateLog.NewVersion.Split(' ')[0]);
                    timer.Join();
                    Model.Status = newVersion > currentVersion ? UpdateStates.Detected : UpdateStates.Latest;
                }
                catch
                {
                    timer.Join();
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
