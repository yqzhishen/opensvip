using System.Windows;
using MaterialDesignThemes.Wpf;

namespace OpenSvip.GUI.Dialog
{
    /// <summary>
    /// AboutDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AboutDialog
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        public static AboutDialog CreateDialog()
        {
            AboutDialog dialog = null;
            Application.Current.Dispatcher.Invoke(() =>
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
    }
}
