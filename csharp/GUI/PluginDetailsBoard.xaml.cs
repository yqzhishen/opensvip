using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using OpenSvip.Framework;
using OpenSvip.GUI.Config;
using OpenSvip.GUI.Dialog;

namespace OpenSvip.GUI
{
    /// <summary>
    /// PluginDetailsBoard.xaml 的交互逻辑
    /// </summary>
    public partial class PluginDetailsBoard : INotifyPropertyChanged
    {
        public PluginDetailsBoard()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _hasUpdate;

        public bool HasUpdate
        {
            get => _hasUpdate;
            set
            {
                _hasUpdate = value;
                OnPropertyChanged();
            }
        }

        private UpdateLog _updateLog;

        private void PluginDetailsBoard_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HasUpdate = false;
            new Thread(() =>
            {
                try
                {
                    Plugin plugin = null;
                    Dispatcher.Invoke(() =>
                    {
                        plugin = (Plugin)DataContext;
                    });
                    if (new UpdateChecker(plugin.UpdateUri).CheckForUpdate(out var updateLog, plugin.Version)
                        && new Version(ConstValues.FrameworkVersion.Split(' ', '-')[0])
                        >= new Version(updateLog.RequiredFrameworkVersion.Split(' ', '-')[0]))
                    {
                        HasUpdate = true;
                        _updateLog = updateLog;
                    }
                }
                catch
                {
                    // ignored
                }
            }).Start();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {

            if (!HasUpdate)
            {
                return;
            }

            ((PopupEx)((Control)Parent).Parent).IsOpen = false;
            PluginDownloadDialog.CreateDialog(_updateLog).ShowDialog();
        }
    }
}