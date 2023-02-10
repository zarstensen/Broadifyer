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
                new Task(async () => await notifier.authUser("token.txt", false)).Start();
            }
        }

        public async void SetupSubscribe()
        {
            notifier.m_event_sub.subscribe(new EventSub.Subscription("stream.online", new() {
                {  "broadcaster_user_id", await notifier.streamerIdFromName("zarstensen")! } }), notifyCallback);

            notifier.m_event_sub.subscribe(new EventSub.Subscription("stream.online", new() {
                {  "broadcaster_user_id", await notifier.streamerIdFromName("andersonjph")! } }), notifyCallback);

            await notifier.m_event_sub.connect();
        }

        public void notifyCallback(NotifyEventArgs args)
        {
            new ToastContentBuilder().AddText($"{args.event_data["broadcaster_user_name"]} Just started streaming {{GAME NAME}}!").AddAppLogoOverride(new($"{Environment.CurrentDirectory}/11fc4351ff82c494-profile_image-70x70.jpeg"), ToastGenericAppLogoCrop.Circle).AddAttributionText("Click to go to stream").Show();
        }

        TwitchNotify notifier = new("mjnfz52170tvwmq4nk1vldg0hufjfv");

    }
}
