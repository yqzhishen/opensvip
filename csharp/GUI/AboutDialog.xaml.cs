using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            DataContext = this;
        }

        private readonly object _lock = new object();

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
