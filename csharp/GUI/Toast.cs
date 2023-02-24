using Microsoft.Toolkit.Uwp.Notifications;
using OpenSvip.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSvip.GUI
{
    public static class Toast
    {
        public static void Show(string title, string message, string defaultAction = "default")
        {
            new ToastContentBuilder()
                .AddArgument("action", defaultAction)
                .AddText(title)
                .AddText(message)
                .Show();
        }

        public static void Show(string title, string message, Uri icon, ToastGenericAppLogoCrop cropType, string defaultAction = "default")
        {
            new ToastContentBuilder()
                .AddArgument("action", defaultAction)
                .AddText(title)
                .AddText(message)
                .AddAppLogoOverride(icon, cropType)
                .Show();
        }
        
        public static void Show(string title, string message, List<ToastActionButton> buttons,
            string defaultAction = "default", List<Tuple<string, string>> args = null)
        {
            var builder = new ToastContentBuilder();

            if (args != null && args.Any())
                foreach (var arg in args)
                    builder.AddArgument(arg.Item1, arg.Item2);

            builder.AddArgument("action", defaultAction)
                   .AddText(title)
                   .AddText(message);
            foreach (var button in buttons)
            {
                var toastButton = new ToastButton();
                toastButton.SetContent(button.Content)
                    .AddArgument("action", button.Action)
                    .SetBackgroundActivation();
                builder.AddButton(toastButton);
            }
            builder.Show();
        }

        public static void ShowUpdateNotifyToast(string title, string message, Plugin plugin)
        {
            var buttons = new List<ToastActionButton>()
            {
                new ToastActionButton("立即更新", "updateNow"),
                new ToastActionButton("下次再说", "dismiss"),
                new ToastActionButton("不再提醒", "neverRemind")
            };
            var args = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("type", "plugin"),
                new Tuple<string, string>("updateUri", plugin.UpdateUri),
                new Tuple<string, string>("version", plugin.Version)
            };
            Show(title, message, buttons, "updateNow", args);
        }

        public static void ShowUpdateNotifyToast(string title, string message)
        {
            var buttons = new List<ToastActionButton>()
            {
                new ToastActionButton("立即更新", "updateNow"),
                new ToastActionButton("下次再说", "dismiss"),
                new ToastActionButton("不再提醒", "neverRemind")
            };
            var args = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("type", "converter")
            };
            Show(title, message, buttons, "updateNow", args);
        }
    }
}