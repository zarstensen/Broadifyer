using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BroadifyerApp.Models;
using TwitchLib.Api.Helix;

namespace BroadifyerApp.ViewModels
{
    public class StreamerVM : ReactiveObject
    {
        public IStreamer Streamer { get => streamer_info.Streamer; }
        public Expose<bool, IStreamerInfo> Enable { get; set; }
        public bool IsLive { get => streamer_info.IsLive ?? false; }
        public string DisplayName { get => streamer_info.Streamer.DisplayName; }
        public string CategoryName { get => streamer_info.CurrentCategory?.Name ?? ""; }
        public Bitmap? Icon { get => IsLive ? streamer_info.RgbIcon : streamer_info.GrayIcon; }
        public Expose<bool, IStreamerInfo> IsWhitelisted { get; set; }

        public void sortCategories()
        {
            FilteredCategories = new(FilteredCategories.OrderBy(x => x.category_info).ToList());
        }

        public ObservableCollection<CategoryVM> FilteredCategories { get; set; } = new();

        public StreamerVM(IStreamerInfo streamer_info)
        {
            this.streamer_info = streamer_info;

            streamer_info.StreamerUpdated += (s, e) =>
            {
                switch (e)
                {
                    case StreamerChange.Broadcast:
                        this.RaisePropertyChanged(nameof(IsLive));
                        this.RaisePropertyChanged(nameof(Icon));
                        break;

                    case StreamerChange.Category:
                        this.RaisePropertyChanged(nameof(CategoryName));
                        break;
                }
            };

            foreach (ICategoryInfo category_info in streamer_info.FilteredCategories.Values)
                FilteredCategories.Add(new(category_info));

            sortCategories();

            Enable = new(streamer_info, nameof(streamer_info.Enable));
            IsWhitelisted = new(streamer_info, nameof(streamer_info.WhitelistCategories));
        }

        protected React<string> m_category_name = new();

        public IStreamerInfo streamer_info;
    }

    public class StreamersViewModel : ViewModelBase
    {
        public string StreamerInput { get; set; } = "";
        public ObservableCollection<StreamerVM> Streamers { get; protected set; } = new();
        public React<StreamerVM?> SelectedStreamer { get; set; } = new();

        public StreamersViewModel(TwitchNotify notifier)
        {
            m_notifier = notifier;

            // incase the notifier has not yet been initialized, also call loadFromNotifier, when (if) the NotifierInitialized event is raised.
            AppVM.NotifierInitialized += (s) => loadFromNotifier();

            loadFromNotifier();
        }

        /// <summary>
        /// attempt to find a streamer with the name stored in StreamerInput, and add it to the TwitchNotify streamer list.
        /// </summary>
        public async Task addStreamer()
        {
            if (StreamerInput == "")
            {
                await WindowVM.showInfo($"Streamer name cannot be empty.", 5000);
                return;
            }

            if (StreamerInput.Contains(' '))
            {
                await WindowVM.showInfo("Streamer name must not contain any whitespace.", 5000);
                return;
            }

            var found_streamer = await m_notifier.streamerFromName(StreamerInput);

            if (found_streamer == null)
            {
                await WindowVM.showInfo($"Could not find a streamer named {StreamerInput}!", 5000);
                return;
            }

            await m_notifier.addStreamers(new() { found_streamer });

            Streamers.Add(createStreamerVM(m_notifier.Streamers[found_streamer.Id]));
            Streamers = new(Streamers.OrderBy(x => x.streamer_info).ToList());

            this.RaisePropertyChanged(nameof(Streamers));
            
            m_notifier.saveConfiguration(AppVM.Settings.ConfigFileName);
        }

        /// <summary>
        /// adds all of the streamers the authenticated user is currently following.
        /// </summary>
        public async Task addFollowedStreamers()
        {
            List<IStreamer> followed_streamers = await m_notifier.followedStreamers();
            await m_notifier.addStreamers(followed_streamers);

            foreach (IStreamer streamer in followed_streamers)
                Streamers.Add(createStreamerVM(m_notifier.Streamers[streamer.Id]));

            Streamers = new(Streamers.OrderBy(x => x.streamer_info).ToList());

            this.RaisePropertyChanged(nameof(Streamers));

            m_notifier.saveConfiguration(AppVM.Settings.ConfigFileName);
        }

        public void removeStreamer(StreamerVM streamer)
        {
            // in case the user has rapidly clicked the remove button, and the command has been fired twice,
            // check if the streamer has already been removed, in order to avoid an exception.
            if (m_notifier.Streamers.ContainsKey(streamer.streamer_info.Streamer.Id))
            {
                m_notifier.removeStreamers(new() { streamer.streamer_info.Streamer });

                streamer.streamer_info.StreamerUpdated -= streamerUpdated;

                Streamers.Remove(streamer);
                this.RaisePropertyChanged(nameof(Streamers));

                m_notifier.saveConfiguration(AppVM.Settings.ConfigFileName);
            }
        }

        // resorts the streamer list, if the broadcast state has changed, as this should change the streamers order in the list.
        protected void streamerUpdated(object? sender, StreamerChange change)
        {
            if(change == StreamerChange.Broadcast)
            {
                Streamers = new(Streamers.OrderBy(x => x.streamer_info).ToList());
                this.RaisePropertyChanged(nameof(Streamers));
            }
        }

        // add all the current TwitchNotify streamers into the Streamers viewmodel, and sort the Streamers list.
        protected void loadFromNotifier()
        {
            foreach (IStreamerInfo streamer_info in m_notifier.currentStreamers())
                Streamers.Add(createStreamerVM(streamer_info));

            Streamers = new(Streamers.OrderBy(x => x.streamer_info).ToList());

            this.RaisePropertyChanged(nameof(Streamers));
        }

        // create a StreamerVM instance, and add an eventhandler for this instance being updated, that resorts the Streamers list.
        protected StreamerVM createStreamerVM(IStreamerInfo sinfo)
        {
            StreamerVM streamer_vm = new(sinfo);
            streamer_vm.streamer_info.StreamerUpdated += streamerUpdated;

            return streamer_vm;
        }

        protected TwitchNotify m_notifier;
        protected StreamerVM? m_selected_streamer;
    }

}
