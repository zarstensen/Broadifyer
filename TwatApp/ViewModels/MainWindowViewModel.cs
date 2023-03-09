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
using Avalonia.Controls.ApplicationLifetimes;
using OpenFileDialog = Avalonia.Controls.OpenFileDialog;
using Newtonsoft.Json;
using SaveFileDialog = Avalonia.Controls.SaveFileDialog;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Reflection;
using System.Threading;
using TwitchLib.Api.Helix.Models.Bits;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Navigation;

namespace TwatApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        public React<ViewModelBase> View { get; set; } = new();

        public React<bool> ShowInfo { get; set; } = false;
        public React<string?> InfoText { get; set; } = "";

        public async Task showInfo(string info, int show_time)
        {
            InfoText.Value = info;
            ShowInfo.Value = true;
            await Task.Delay(show_time);
            ShowInfo.Value = false;
        }

        /// <summary>
        /// retrieve a sorted list of the current streamers.
        /// sorted according to live status, followed by streamer display name.
        /// </summary>

        public TwitchNotify notifier;
        public string VersionNumber { get
            {
                string location;

                Assembly assembly = Assembly.GetExecutingAssembly();

                if (assembly.Location != string.Empty)
                    location = assembly.Location;
                else
                    location = Environment.ProcessPath;

                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(location);
                string version = fileVersionInfo.ProductVersion;

                return $"Version: {version}";
            }
        }

        public MainWindowViewModel()
        {
            Trace.WriteLine(VersionNumber);
            // the TwitchNotify instance is retrieved from the app, as this instance needs to be active, even when this window is closed,
            // so its lifetime is therefore not tied to this windows lifetime.

            notifier = AppVM.notifier;

            m_config_view_model = new(notifier);

            View.Value = m_config_view_model;
        }

        public void showSettingsView()
        {
            SettingsViewModel settings_view_model = new SettingsViewModel(notifier);
            View.Value = settings_view_model;

            settings_view_model.Exit.Subscribe(
                x => View.Value = m_config_view_model
                );
        }

        public async void importConfig()
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                FileDialogFilter json_filter = new FileDialogFilter { Name = "Json file", Extensions = new List<string> { "json" } };
                OpenFileDialog dialog = new OpenFileDialog();

                dialog.Filters.Add(json_filter);
                
                var res = await dialog.ShowAsync(desktop.MainWindow);

                if (res == null)
                    return;

                string file = res[0];

                try
                {
                    await notifier.loadConfiguration(file);
                    File.Copy(file, AppVM.settings.ConfigFileName, true);
                }
                catch(Exception ex)
                {
                    Trace.WriteLine(ex);
                }

                View.Value = m_config_view_model = new(notifier);
            }
        }

        public async void exportConfig()
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                FileDialogFilter json_filter = new FileDialogFilter { Name = "Json file", Extensions = new List<string> { "json" } };
                SaveFileDialog dialog = new SaveFileDialog();

                dialog.Filters.Add(json_filter);

                var res = await dialog.ShowAsync(desktop.MainWindow);

                if (res == null)
                    return;

                try
                {
                    notifier.saveConfiguration(res);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }

        public async void reloadCache()
        {
            await AppVM.notifier.reloadAllCache();
            AppVM.notifier.saveConfiguration(AppVM.settings.ConfigFileName);
            View.Value = m_config_view_model = new ConfigEditorViewModel(AppVM.notifier);
        }

        protected ConfigEditorViewModel m_config_view_model;

    }
}
