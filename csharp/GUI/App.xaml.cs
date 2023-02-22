using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using Windows.Foundation.Collections;
using MaterialDesignThemes.Wpf;
using Microsoft.Toolkit.Uwp.Notifications;
using OpenSvip.GUI.Config;
using OpenSvip.GUI.Dialog;
using OpenSvip.Framework;
using System;

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
            RunToastBackground();
        }

        private static void RunToastBackground()
        {
            // Listen to notification activation
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                var args = ToastArguments.Parse(toastArgs.Argument);
                //var userInput = toastArgs.UserInput;
                var action = args.Get("action");
                switch (action)
                {
                    case "updateNow":
                        new Thread(() =>
                        {
                            try
                            {
                                new UpdateChecker(args.Get("updateUri")).CheckForUpdate(out var updateLog, args.Get("version"));
                                Application.Current.Dispatcher.Invoke(delegate
                                {
                                    PluginDownloadDialog.CreateDialog(updateLog).ShowDialog();
                                });
                            }
                            catch
                            {
                                // ignored
                            }
                        }).Start();
                        break;
                    case "dismiss":
                        //ignore
                        break;
                    case "neverRemind":
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            ((MainWindow)Current.MainWindow)?.Model.DisableCheckForUpdatesOnStartUp();
                        });
                        
                        ToastNotificationManagerCompat.Uninstall(); //Uninstall toast notification.
                        break;
                }
            };
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
