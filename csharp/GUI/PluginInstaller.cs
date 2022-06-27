using System;
using System.IO;
using System.Windows;
using OpenSvip.Framework;
using OpenSvip.GUI.Dialog;

namespace OpenSvip.GUI
{
    public class PluginInstaller
    {
        public int Success { get; private set; }

        public void Install(params string[] paths)
        {
            foreach (var path in paths)
            {
                try
                {
                    var plugin = PluginManager.ExtractPlugin(path, out var folder);
                    YesNoDialog confirmDialog;
                    if (new Version(ConstValues.FrameworkVersion) < new Version(plugin.TargetFramework))
                    {
                        MessageDialog.CreateDialog(
                            "不兼容的插件版本",
                            $"当前应用版本过旧，无法安装插件“{plugin.Name}”。请使用菜单栏“帮助-检查更新”升级应用后再次尝试。")
                            .ShowDialog();
                        continue;
                    }
                    if (!PluginManager.HasPlugin(plugin.Identifier))
                    {
                        confirmDialog = YesNoDialog.CreateDialog(
                            "安装新的插件",
                            $"将要安装由 {plugin.Author} 开发，适用于 {plugin.Format} (*.{plugin.Suffix}) 的插件“{plugin.Name}”。\n确认继续？",
                            "安装");
                        if (!confirmDialog.ShowDialog())
                        {
                            continue;
                        }
                        try
                        {
                            PluginManager.InstallPlugin(plugin, folder);
                        }
                        catch (PluginFolderConflictException e)
                        {
                            confirmDialog = YesNoDialog.CreateDialog(
                                "插件文件夹冲突",
                                $"试图安装一个新的插件，但其文件夹“{e.FolderName}”与已有插件冲突。\n出现此问题可能是由于插件开发者修改了插件的标识符；若您无法确认上述情况，继续安装可能会导致丢失插件数据。\n确认要覆盖安装吗？",
                                "覆盖");
                            if (!confirmDialog.ShowDialog())
                            {
                                continue;
                            }
                            PluginManager.InstallPlugin(plugin, folder, true);
                        }
                        ++Success;
                        continue;
                    }
                    var oldPlugin = PluginManager.GetPlugin(plugin.Identifier);
                    var oldVersion = new Version(oldPlugin.Version);
                    var version = new Version(plugin.Version);
                    if (version > oldVersion)
                    {
                        confirmDialog = YesNoDialog.CreateDialog(
                            "更新已有插件",
                            $"插件“{plugin.Name}”将由 {oldVersion} 更新至 {version}。\n确认继续？",
                            "更新");
                    }
                    else
                    {
                        confirmDialog = YesNoDialog.CreateDialog(
                            "此插件不是新的版本",
                            $"当前已安装插件“{plugin.Name}”的相同或更新版本 ({oldVersion} ≥ {version})。\n确认要覆盖安装吗？",
                            "覆盖");
                    }
                    if (!confirmDialog.ShowDialog())
                    {
                        continue;
                    }
                    PluginManager.InstallPlugin(plugin, folder);
                    ++Success;
                }
                catch (Exception e)
                {
                    MessageDialog.CreateDialog("插件安装失败", e.Message).ShowDialog();
                }
            }
            if (Directory.Exists(PluginManager.TempPath))
            {
                new DirectoryInfo(PluginManager.TempPath).Delete(true);
            }
            if (Success <= 0)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                ((MainWindow)Application.Current.MainWindow)?.Model.RefreshPluginsSource();
            });
            MessageDialog.CreateDialog(
                "插件安装完成",
                $"已成功安装 {Success} 个插件。新的功能已准备就绪。").ShowDialog();
        }
    }
}
