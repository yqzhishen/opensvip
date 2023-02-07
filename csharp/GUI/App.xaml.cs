using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace OpenSvip.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception;
            MessageBox.Show($"{exception.Message}\n{exception.StackTrace}");
        }

        private bool _appUsesLightMode;

        public bool AppUsesLightMode
        {
            get => _appUsesLightMode;
            set
            {
                _appUsesLightMode = value;
                new Thread(() =>
                {
                    var theme = (BundledTheme)Current.Resources.MergedDictionaries
                        .First(rd => rd.GetType() == typeof(BundledTheme));
                    theme.BaseTheme = value ? BaseTheme.Light : BaseTheme.Dark;
                    OnPropertyChanged();
                }).Start();
            }
        }

        /*
         * 以下代码用于移除所有元素的 FocusVisualStyle，即被 Focus 时周围出现的虚线框。
         * 作者：Tromse
         * 来源：https://stackoverflow.com/questions/1055670/deactivate-focusvisualstyle-globally
         */
        private void Application_StartUp(object sender, StartupEventArgs e)
        {
            EventManager.RegisterClassHandler(typeof(FrameworkElement), UIElement.GotFocusEvent, new RoutedEventHandler(RemoveFocusVisualStyle), true);
        }

        private static void RemoveFocusVisualStyle(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).FocusVisualStyle = null;
        }
    }
}
