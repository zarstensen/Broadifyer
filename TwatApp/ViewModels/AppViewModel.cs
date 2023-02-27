using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwatApp.Models;
using TwitchLib.Api.Helix;

namespace TwatApp.ViewModels
{
    
    public class AppViewModel : ViewModelBase
    {
        public AppViewModel()
        {
            // this code should not be called, if in design mode, as neither a notification nor a twitch api call will be made, during design mode.
            if (Design.IsDesignMode)
                return;

            new Task(async () =>
            {
                await notifier.authUser("token.txt", false);
                await notifier.loadConfiguration("config.json");
                notifier.PollInterval = 1;
                notifier.StreamerNotify += notifyUser;
                notifier.startNotify();
            }).Start();

            ToastNotificationManagerCompat.OnActivated += toast_args =>
            {
                ToastArguments args = ToastArguments.Parse(toast_args.Argument);

                Process.Start(new ProcessStartInfo() { FileName = $"https://www.twitch.tv/{args["streamer"]}", UseShellExecute = true });
            };

            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Exit += (s, e) => onExit();
            }
        }

        /// <summary>
        /// gets called on application exit, used for handeling cleanup of the toast notifications, and for saving the current streamer notification configurations.
        /// </summary>
        private void onExit()
        {
            ToastNotificationManagerCompat.History.Clear();
            ToastNotificationManagerCompat.Uninstall();
            notifier.saveConfiguration("config.json");
        }

        /// <summary>
        /// shutsdown the entire application, and removes the app from the tray.
        /// </summary>
        public void ExitCommand()
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.TryShutdown();
                onExit();
            }
        }

        public void EditorWindowCommand()
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow.Show();
        }

        public void notifyUser(object? sender, IStreamerInfo streamer_info)
        {
            new ToastContentBuilder().
                AddText($"{streamer_info.Streamer.DisplayName} Just started streaming {streamer_info.CurrentCategory?.Name ?? ""}!").
                AddAppLogoOverride(new(streamer_info.Streamer.IconFileOnline), ToastGenericAppLogoCrop.Circle).
                AddAttributionText("Click to go to stream").
                AddArgument("streamer", streamer_info.Streamer.LoginName).
                Show();
        }

        public TwitchNotify notifier = new("mjnfz52170tvwmq4nk1vldg0hufjfv");
    }
}
