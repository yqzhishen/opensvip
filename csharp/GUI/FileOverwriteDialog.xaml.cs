using MaterialDesignThemes.Wpf;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace OpenSvip.GUI
{
    /// <summary>
    /// FileOverwriteDialog.xaml 的交互逻辑
    /// </summary>
    public partial class FileOverwriteDialog
    {
        private bool _isOpen;

        public string OverwrittenPath
        {
            get => OverwrittenPathTextBlock.Text;
            set
            {
                Dispatcher.Invoke(() =>
                {
                    OverwrittenPathTextBlock.Text = $"文件“{value}”已存在。";
                });
            }
        }

        public bool Overwrite { get; private set; }

        public bool KeepChoice { get; private set; }

        public static FileOverwriteDialog CreateDialog(string overwrittenPath)
        {
            FileOverwriteDialog dialog = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                dialog = new FileOverwriteDialog
                {
                    OverwrittenPath = overwrittenPath
                };
            });
            return dialog;
        }

        public FileOverwriteDialog()
        {
            InitializeComponent();
        }

        public void ShowDialog()
        {
            _isOpen = true;
            Dispatcher.Invoke(() =>
            {
                DialogHost.Show(this, "RootDialogHost");
            });
            while (_isOpen)
            {
                Thread.Sleep(0);
            }
            Dispatcher.Invoke(() =>
            {
                KeepChoice = KeepChoiceCheckBox.IsChecked ?? false;
            });
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            Overwrite = (bool) ((Button) sender).CommandParameter;
            _isOpen = false;
        }
    }
}
