using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace OpenSvip.GUI
{
    /// <summary>
    /// AboutDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AboutDialog : UserControl
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        public static AboutDialog CreateDialog()
        {
            AboutDialog dialog = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                dialog = new AboutDialog();
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

        public void OpenLinkButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Button)sender).ToolTip.ToString());
        }
    }
}
