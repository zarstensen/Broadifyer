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

namespace TwatApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        string StartupPath = $"\"{Environment.ProcessPath}\" --minimized";
        public bool RunsOnStartup
        {
            get
            {
                RegistryKey? run_key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (run_key == null)
                    return false;

                return (string?)run_key.GetValue("Twats") == StartupPath;
            }
            set
            {
                if (RunsOnStartup == value)
                    return;

                RegistryKey? run_key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (!value)
                    run_key!.DeleteValue("Twats");
                else
                    run_key!.SetValue("Twats", StartupPath!);
            }
        }


        public string RunOnStartupHeader
        {
            get
            {
                if (RunsOnStartup)
                    return "[x] Run On Startup";
                else
                    return "[  ] Run On Startup";
            }
        }

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

            View.Value = new ConfigEditorViewModel(notifier);
        }

        public void toggleRunOnStartup()
        {
            RunsOnStartup = !RunsOnStartup;
            this.RaisePropertyChanged(nameof(RunOnStartupHeader));
        }

    }
}
