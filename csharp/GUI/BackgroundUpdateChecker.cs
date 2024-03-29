﻿using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Threading;
using OpenSvip.Framework;
using Panuon.UI.Silver;

namespace OpenSvip.GUI
{
    public static class BackgroundUpdateChecker
    {
        public static void CheckAppUpdate()
        {
            new Thread(() =>
            {
                try
                {
                    if (new UpdateChecker().CheckForUpdate(out var updateLog))
                    {
                        var title = $"OpenSVIP v{updateLog.Version} 更新";
                        var message = string.Join("\n", updateLog.Items);
                        Toast.ShowUpdateNotifyToast(title, message);
                    }
                }
                catch
                {
                    // ignored
                }
            }).Start();
        }

        public static void CheckPluginsUpdate()
        {
            
            new Thread(() =>
            {
                try
                {
                    var plugins = PluginManager.GetAllPlugins();
                    foreach (var plugin in plugins)
                    {
                        if (new UpdateChecker(plugin.UpdateUri).CheckForUpdate(out var updateLog, plugin.Version)
                            && new Version(ConstValues.FrameworkVersion.Split(' ', '-')[0])
                            >= new Version(updateLog.RequiredFrameworkVersion.Split(' ', '-')[0]))
                        {
                            var title = $"{plugin.Name} v{updateLog.Version} 更新";
                            var message = string.Join("\n", updateLog.Items);
                            Toast.ShowUpdateNotifyToast(title, message, plugin);
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }).Start();
        }

    }
}