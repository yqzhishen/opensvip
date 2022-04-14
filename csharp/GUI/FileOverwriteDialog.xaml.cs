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
        public FileOverwriteDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private static bool _keepChoice;

        private readonly object _lock = new object();

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

        public bool Overwrite { get; set; }

        public bool KeepChoice
        {
            get => _keepChoice;
            set => _keepChoice = value;
        }

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

        public void ShowDialog()
        {
            Dispatcher.Invoke(() =>
            {
                Monitor.Enter(_lock);
                DialogHost.Show(this, "RootDialogHost");
            });
            Monitor.Enter(_lock);
            Dispatcher.Invoke(() =>
            {
                KeepChoice = KeepChoiceCheckBox.IsChecked ?? false;
            });
            Monitor.Exit(_lock);
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            Overwrite = (bool) ((Button) sender).CommandParameter;
            Monitor.Exit(_lock);
        }
    }
}
