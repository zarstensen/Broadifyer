using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TwatApp.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
using ReactiveUI;
using Avalonia.Collections;
using System.Collections.ObjectModel;

namespace TwatApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// property containing the string value in the streamer name input field, for the StreamerSection control.
        /// </summary>
        public React<string> StreamerInput { get; set; } = "";
        public React<string> CategoryInput { get; set; } = "";
        /// <summary>
        /// property containing the currently selected streamer, in the StreamerSection streamer listbox.
        /// </summary>
        public React<StreamerViewModel?> SelectedStreamer { get; set; } = new();
        
        /// <summary>
        /// retrieve a sorted list of the current streamers.
        /// sorted according to live status, followed by streamer display name.
        /// </summary>
        public AvaloniaList<StreamerViewModel> Streamers
        {
            get
            {
                var sorted_streamers = notifier.currentStreamers();
                sorted_streamers.Sort();
                return new(sorted_streamers.Select(x => new StreamerViewModel(x)));
            }
            set
            {
                Trace.WriteLine(value);
            }
        }
        
        public TwitchNotify notifier;

        public MainWindowViewModel()
        {
            // the TwitchNotify instance is retrieved from the app, as this instance needs to be active, even when this window is closed,
            // so its lifetime is therefore not tied to this windows lifetime.

            notifier = (App.Current!.DataContext as AppViewModel)!.notifier;

            if (!Design.IsDesignMode)
            {
                notifier.StreamerChanged += (s, e) => this.RaisePropertyChanged(nameof(Streamers));
            }
        }

        /// <summary>
        /// attempt to find a streamer with the name stored in StreamerInput, and add it to the TwitchNotify streamer list.
        /// </summary>
        public async void addStreamer()
        {
            if (StreamerInput.Value == "" || StreamerInput.Value.Contains(' '))
                return;

            var found_streamer = await notifier.streamerFromName(StreamerInput.Value);

            if (found_streamer == null)
            {
                // do some error handling
                return;
            }
            
            await notifier.addStreamers(new() { found_streamer });
            this.RaisePropertyChanged(nameof(Streamers));
            Trace.WriteLine(found_streamer);
        }

        /// <summary>
        /// adds all of the streamers the authenticated user is currently following.
        /// </summary>
        public async Task addFollowedStreamers()
        {
            await notifier.addStreamers(await notifier.followedStreamers());
            this.RaisePropertyChanged(nameof(Streamers));
        }

        /// <summary>
        /// attempt to add a category with the name stored in CategoryInput, and associate it with the passed streamer.
        /// </summary>
        /// <param name="streamer"></param>
        public async void addCategory()
        {
            if (CategoryInput.Value == "" || CategoryInput.Value.Contains(' '))
                return;

            var found_category = await notifier.categoryFromName(CategoryInput);

            if(found_category == null || SelectedStreamer.Value == null)
            {
                return;
            }

            notifier.filterCategory(found_category, SelectedStreamer.Value.streamer_info.Streamer);
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
    }
}
