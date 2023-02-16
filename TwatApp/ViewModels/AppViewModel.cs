using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwatApp.ViewModels
{
    
    public class AppViewModel : ViewModelBase
    {
        public AppViewModel()
        {
            if (Design.IsDesignMode)
                return;

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
        }

        public void ExitCommand()
        {
            IClassicDesktopStyleApplicationLifetime lifetime = (IClassicDesktopStyleApplicationLifetime)App.Current!.ApplicationLifetime!;
            lifetime.TryShutdown();
        }
    }
}
