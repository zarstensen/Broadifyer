using Avalonia.Controls;
using EventSub;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TwatApp.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;

namespace TwatApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";

        public MainWindowViewModel()
        {

            if (!Design.IsDesignMode)
            {
                new Task(async () =>
                    {
                        await notifier.authUser("token.txt", false);
                        notifier.PollInterval = 1;
                        notifier.startNotify();
                        //string uri = await notifier.streamerIcon(await notifier.streamerIdFromName("dougdougw"), "icon.png")!;
                        //    Trace.WriteLine(uri);
                        //    new ToastContentBuilder().AddText($"Just started streaming {{GAME NAME}}!").AddAppLogoOverride(new(uri), ToastGenericAppLogoCrop.Circle).AddAttributionText("Click to go to stream").Show();
                        //}).Start();
                    }).Start();
                
            }


        }

        public async void SetupSubscribe()
        {
            await notifier.addStreamers(await notifier.followedStreamers());
            await notifier.addStreamers(await notifier.streamersFromNames(new() { "zarstensen" }));
            notifier.saveConfiguration("config.json");
            IStreamer streamer = (await notifier.streamersFromNames(new() { "zarstensen" }))[0];
            notifier.Streamers[streamer.Id].Disable = true;
        }

        public void notifyCallback(NotifyEventArgs args)
        {
        }

        TwitchNotify notifier = new("mjnfz52170tvwmq4nk1vldg0hufjfv");

    }
}
