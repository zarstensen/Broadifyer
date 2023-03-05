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
using Microsoft.Win32;
using System.Reactive.Linq;

namespace TwatApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        public React<ViewModelBase> View { get; set; } = new();


        /// <summary>
        /// retrieve a sorted list of the current streamers.
        /// sorted according to live status, followed by streamer display name.
        /// </summary>
        
        public TwitchNotify notifier;

        public MainWindowViewModel()
        {
            // the TwitchNotify instance is retrieved from the app, as this instance needs to be active, even when this window is closed,
            // so its lifetime is therefore not tied to this windows lifetime.

            notifier = (App.Current!.DataContext as AppViewModel)!.notifier;

            m_config_view_model = new(notifier);

            View.Value = m_config_view_model;

            showSettingsView();
        }

        public void showSettingsView()
        {
            SettingsViewModel settings_view_model = new SettingsViewModel(notifier);
            View.Value = settings_view_model;

            settings_view_model.Exit.Subscribe(
                x => View.Value = m_config_view_model
                );
        }

        protected ConfigEditorViewModel m_config_view_model;

    }
}
