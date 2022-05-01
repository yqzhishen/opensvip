using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

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
    }
}
