using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Broadifyer.Models;
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
using System.Windows.Navigation;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using DynamicData.Aggregation;
using Broadifyer.Views;

namespace Broadifyer.ViewModels
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
        

        public MainWindowViewModel()
        {
            Trace.WriteLine(AppVM.VersionString);
            // the TwitchNotify instance is retrieved from the app, as this instance needs to be active, even when this window is closed,
            // so its lifetime is therefore not tied to this windows lifetime.

            notifier = AppVM.notifier;

            m_config_view_model = new(notifier);

            View.Value = m_config_view_model;
        }

        /// <summary>
        /// Allows the user to change their associated twitch account, by re authorizing with a force verify flag.
        /// </summary>
        public async Task changeAccount()
        {
            if (!await notifier.authUser(AppVM.Settings.TokenFile, true))
                await showInfo("Failed to change account.", 5000);
        }

        public void showSettingsView()
        {
            SettingsViewModel settings_view_model = new SettingsViewModel(notifier);
            View.Value = settings_view_model;

            settings_view_model.Exit.Subscribe(
                x => View.Value = m_config_view_model
                );
        }

        public async Task importConfig()
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                FileDialogFilter json_filter = new FileDialogFilter { Name = "Json file", Extensions = new List<string> { "json" } };
                OpenFileDialog dialog = new OpenFileDialog();

                dialog.Filters!.Add(json_filter);
                
                var res = await dialog.ShowAsync(desktop.MainWindow);

                if (res == null)
                    return;

                string file = res[0];

                try
                {
                    await notifier.loadConfiguration(file);
                    File.Copy(file, AppVM.Settings.ConfigFileName, true);
                }
                catch(Exception ex)
                {
                    Trace.WriteLine(ex);
                }

                View.Value = m_config_view_model = new(notifier);
            }
        }

        public async Task exportConfig()
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                FileDialogFilter json_filter = new FileDialogFilter { Name = "Json file", Extensions = new List<string> { "json" } };
                SaveFileDialog dialog = new SaveFileDialog();

                dialog.Filters!.Add(json_filter);

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

        public async Task reloadCache()
        {
            await AppVM.notifier.reloadAllCache();
            AppVM.notifier.saveConfiguration(AppVM.Settings.ConfigFileName);
            View.Value = m_config_view_model = new ConfigEditorViewModel(AppVM.notifier);
        }

        public async Task autoUpdate()
        {
            var update_window = new UpdateWindow();

            update_window.DataContext = new UpdateViewModel();

            update_window.Show();

            //if(await checkNewVersion() ?? false)
            //{
            //    await downloadLatestRelease("release.zip");
            //    installRelease("release.zip");
            //}
        }

        protected ConfigEditorViewModel m_config_view_model;
        protected HttpClient m_http_client = new();

    }
}
