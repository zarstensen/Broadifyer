using Avalonia.Controls;
using EventSub;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TwatApp.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
using ReactiveUI;

namespace TwatApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";

        public MainWindowViewModel()
        {
            notifier = (App.Current!.DataContext as AppViewModel)!.notifier;

            if (!Design.IsDesignMode)
            {
                notifier.StreamerChanged += (sender, arg) => this.RaisePropertyChanged(nameof(Streamers));
            }
        }

        public async void SetupSubscribe()
        { 
            await notifier.addStreamers(await notifier.streamersFromNames(new() { "zarstensen" }));
            notifier.saveConfiguration("config.json");
            IStreamer streamer = (await notifier.streamersFromNames(new() { "zarstensen" }))[0];
            notifier.Streamers[streamer.Id].Disable = true;
        }

        public async void addCommand()
        {
            if (streamer.Value == "" || streamer.Value.Contains(' '))
                return;

            var found_streamer = await notifier.streamerFromName(streamer);

            if (found_streamer == null)
            {
                // do some error handling
            }
            else
            {
                await notifier.addStreamers(new() { found_streamer });
                this.RaisePropertyChanged(nameof(Streamers));
                Trace.WriteLine(found_streamer.IconFile);
            }

            foreach(IStreamerInfo streamer in Streamers)
            {
                Trace.WriteLine(streamer.Streamer.IconFile);
            }
        }

        public async Task addFollowedCommand()
        {
            await notifier.addStreamers(await notifier.followedStreamers());
            this.RaisePropertyChanged(nameof(Streamers));
        }

        public void removeStreamer(IStreamer streamer)
        {
            // in case the user has rapidly clicked the remove button, and the command has been fired twice,
            // check if the streamer has already been removed, in order to avoid an exception.
            if (notifier.Streamers.ContainsKey(streamer.Id))
            {
                notifier.removeStreamers(new() { streamer });
                this.RaisePropertyChanged(nameof(Streamers));
            }
        }

        public void toggleDisable(Button control)
        {
            if (control.DataContext is IStreamerInfo streamer_info)
            {
                streamer_info.Disable = !streamer_info.Disable;
                // converter is not called here for some reason, so we must change the content manually
                control.Content = streamer_info.Disable ? "ENABLE" : "DISABLE";
                this.RaisePropertyChanged();
            }
        }

        public void SelectedStreamerChanged(object? sender, SelectionChangedEventArgs args)
        {
            StreamerName = StreamerName + "EEEEEE";
            this.RaisePropertyChanged(nameof(SelectedStreamer));
            this.RaisePropertyChanged(nameof(StreamerName));
        }

        TwitchNotify notifier;
        public React<string> streamer { get; set; } = new();
        public React<string?> StreamerName { get; set; } = new();
        public React<IStreamerInfo?> SelectedStreamer { get; set; } = new();
        public List<IStreamerInfo> Streamers
        {
            get
            {
                var sorted_streamers = notifier.currentStreamers();
                sorted_streamers.Sort();
                return sorted_streamers;    
            }
        }

    }
}
