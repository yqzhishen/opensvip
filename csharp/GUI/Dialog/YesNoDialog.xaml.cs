using System.Threading;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace OpenSvip.GUI.Dialog
{
    /// <summary>
    /// YesNoDialog.xaml 的交互逻辑
    /// </summary>
    public partial class YesNoDialog
    {
        private readonly object _lock = new object();

        public string Title { get; set; }

        public string Message { get; set; }

        public string YesText { get; set; } = "确定";

        public string NoText { get; set; } = "取消";

        private bool _yes;

        public YesNoDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static YesNoDialog CreateDialog(string title, string message, string yesText = "确定", string noText = "取消")
        {
            YesNoDialog dialog = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                dialog = new YesNoDialog
                {
                    Title = title,
                    Message = message,
                    YesText = yesText,
                    NoText = noText
                };
            });
            return dialog;
        }

        public bool ShowDialog()
        {
            Dispatcher.Invoke(() =>
            {
                Monitor.Enter(_lock);
                DialogHost.Show(this, "RootDialogHost");
            });
            Monitor.Enter(_lock);
            Monitor.Exit(_lock);
            return _yes;
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            _yes = (bool)((Button)sender).CommandParameter;
            Monitor.Exit(_lock);
        }
    }
}
