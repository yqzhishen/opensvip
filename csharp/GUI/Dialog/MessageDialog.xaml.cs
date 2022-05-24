using System;
using System.Threading;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace OpenSvip.GUI.Dialog
{
    /// <summary>
    /// MessageDialog.xaml 的交互逻辑
    /// </summary>
    public partial class MessageDialog
    {
        private readonly object _lock = new object();

        public string Title { get; private set; }

        public string Message { get; private set; }

        public string ButtonText { get; private set; } = "确定";
        
        public Action ButtonAction { get; private set; }

        public MessageDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static MessageDialog CreateDialog(
            string title,
            string message,
            string buttonText = "确定",
            Action buttonAction = null)
        {
            MessageDialog dialog = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                dialog = new MessageDialog
                {
                    Title = title,
                    Message = message,
                    ButtonText = buttonText,
                    ButtonAction = buttonAction
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Monitor.Exit(_lock);
            ButtonAction?.Invoke();
        }
    }
}
