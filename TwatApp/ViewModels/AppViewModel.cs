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
            if (Design.IsDesignMode)
                return;

            new Task(async () =>
            {
                await notifier.authUser("token.txt", false);
                await notifier.loadConfiguration("config.json");
                notifier.PollInterval = 1;
                notifier.startNotify();
            }).Start();

            ToastNotificationManagerCompat.OnActivated += toast_args =>
            {
                ToastArguments args = ToastArguments.Parse(toast_args.Argument);

                Process.Start(new ProcessStartInfo() { FileName = $"https://www.twitch.tv/{args["streamer"]}", UseShellExecute = true });
            };

            (App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.Exit += exitEvent;
        }

        private void exitEvent(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            ToastNotificationManagerCompat.History.Clear();
            ToastNotificationManagerCompat.Uninstall();
            notifier.saveConfiguration("config.json");
        }

        public void ExitCommand()
        {
            IClassicDesktopStyleApplicationLifetime lifetime = (IClassicDesktopStyleApplicationLifetime)App.Current!.ApplicationLifetime!;
            lifetime.TryShutdown();
        }

        public TwitchNotify notifier = new("mjnfz52170tvwmq4nk1vldg0hufjfv");
    }
}
