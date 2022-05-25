using System;
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

        public string Title { get; private set; }

        public string Message { get; private set; }

        public string YesText { get; private set; } = "确定";

        public string NoText { get; private set; } = "取消";

        public Action YesAction { get; private set; }
        
        public Action NoAction { get; private set; }

        private bool _yes;

        public YesNoDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static YesNoDialog CreateDialog(
            string title,
            string message,
            string yesText = "确定",
            string noText = "取消",
            Action yesAction = null,
            Action noAction = null)
        {
            YesNoDialog dialog = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                dialog = new YesNoDialog
                {
                    Title = title,
                    Message = message,
                    YesText = yesText,
                    NoText = noText,
                    YesAction = yesAction,
                    NoAction = noAction
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            _yes = (bool)button.CommandParameter;
            Monitor.Exit(_lock);
            if (_yes)
            {
                YesAction?.Invoke();
            }
            else
            {
                NoAction?.Invoke();
            }
        }
    }
}
