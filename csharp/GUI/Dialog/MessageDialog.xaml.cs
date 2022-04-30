using System.Threading;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace OpenSvip.GUI.Dialog
{
    /// <summary>
    /// MessageDialog.xaml 的交互逻辑
    /// </summary>
    public partial class MessageDialog : UserControl
    {
        private readonly object _lock = new object();

        public string Title { get; set; }

        public string Message { get; set; }

        public string ButtonText { get; set; } = "确定";

        public MessageDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static MessageDialog CreateDialog(string title, string message, string buttonText = "确定")
        {
            MessageDialog dialog = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                dialog = new MessageDialog
                {
                    Title = title,
                    Message = message,
                    ButtonText = buttonText
                };
            });
            return dialog;
        }

        public void ShowDialog()
        {
            Dispatcher.Invoke(() =>
            {
                Monitor.Enter(_lock);
                DialogHost.Show(this, "RootDialogHost");
            });
            Monitor.Enter(_lock);
            Monitor.Exit(_lock);
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            Monitor.Exit(_lock);
        }
    }
}
