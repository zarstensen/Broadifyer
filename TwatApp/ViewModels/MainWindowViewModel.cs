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
        public React<ViewModelBase> View { get; set; } = new();

        /// <summary>
        /// property containing the string value in the streamer name input field, for the StreamerSection control.
        /// </summary>
        public React<string> StreamerInput { get; set; } = "";
        public React<string> CategoryInput { get; set; } = "";
        /// <summary>
        /// property containing the currently selected streamer, in the StreamerSection streamer listbox.
        /// </summary>
        public React<StreamerVM?> SelectedStreamer { get; set; } = new();

        /// <summary>
        /// retrieve a sorted list of the current streamers.
        /// sorted according to live status, followed by streamer display name.
        /// </summary>
        public ObservableCollection<StreamerVM> Streamers
        {
            get
            {
                return m_streamers;
            }
        }
        
        public TwitchNotify notifier;

        public MainWindowViewModel()
        {
            // the TwitchNotify instance is retrieved from the app, as this instance needs to be active, even when this window is closed,
            // so its lifetime is therefore not tied to this windows lifetime.

            notifier = (App.Current!.DataContext as AppViewModel)!.notifier;

            View.Value = new ConfigEditorViewModel(notifier);

            //m_streamers = new();

            //foreach (IStreamerInfo streamer_info in notifier.currentStreamers())
            //    m_streamers.Add(new(streamer_info));

            //new Task(async () => await logSelected()).Start();
        }

        public async Task logSelected()
        {
            Trace.WriteLine("BEGUN LOGGING");
            while (true)
            {
                this.RaisePropertyChanged(nameof(Streamers));
                //Trace.WriteLine($"STREAMER: {SelectedStreamer.Value}");
                await Task.Delay(5500);
            }
        }

        /// <summary>
        /// attempt to add a category with the name stored in CategoryInput, and associate it with the passed streamer.
        /// </summary>
        /// <param name="streamer"></param>
        public async void addCategory()
        {
            if (CategoryInput.Value == "")
                return;

            var found_category = await notifier.categoryFromName(CategoryInput);

            if(found_category == null || SelectedStreamer.Value == null)
            {
                return;
            }

            var cinfo = notifier.filterCategory(found_category, SelectedStreamer.Value.streamer_info.Streamer);
            this.RaisePropertyChanged(nameof(SelectedStreamer.Value));

            await cinfo.prepareIcons();

            SelectedStreamer.Value.FilteredCategories.Add(new(cinfo));
            notifier.saveConfiguration("config.json");
        }

        public void removeCategory(CategoryViewModel category)
        {
            SelectedStreamer.Value.FilteredCategories.Remove(category);
            SelectedStreamer.Value.streamer_info.FilteredCategories.Remove(category.category_info.Category.Id);
            notifier.saveConfiguration("config.json");
        }

        protected ObservableCollection<StreamerVM> m_streamers;
    }
}
