using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace OpenSvip.GUI
{
    /// <summary>
    /// PluginDetailsBoard.xaml 的交互逻辑
    /// </summary>
    public partial class PluginDetailsBoard : UserControl
    {
        public PluginDetailsBoard()
        {
            InitializeComponent();
        }

        private void AuthorHomePageLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }
    }
}
