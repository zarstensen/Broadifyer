using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwatApp.Models;
using TwitchLib.Api.Helix;

namespace TwatApp.ViewModels
{
    
    public class AppViewModel : ViewModelBase
    {

        public class Settings
        {
            public Settings(TwitchNotify notifier)
            {
                m_notifier = notifier;
            }

            [JsonIgnore]
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

            public string ConfigFileName {
                get => m_config_file_name;
                set
                {
                    if (File.Exists(m_config_file_name) && m_config_file_name != value)
                        File.Copy(m_config_file_name, value, true);

                    m_config_file_name = value;
                }
            }
            public bool UseTokenFile { get => m_use_token_file;
                set
                {
                    if (!value && File.Exists(TokenFile))
                        File.Delete(TokenFile);

                    m_use_token_file = value;
                }
            }

            [JsonIgnore]
            public string TokenFile { get; } = "token.txt";
            public int PollInterval { get => m_notifier.PollInterval; set => m_notifier.PollInterval = value; }
            public int NewBroadcastTimeout { get => m_notifier.NewBroadcastTimeout; set => m_notifier.NewBroadcastTimeout = value; }

            public void save()
            {
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(this, Formatting.Indented));
            }

            protected TwitchNotify m_notifier;
            protected bool m_use_token_file = false;
            protected string m_config_file_name = "config.json";
        }

        public Settings settings { get; set; }

        public AppViewModel()
        {
            // this code should not be called, if in design mode, as neither a notification nor a twitch api call will be made, during design mode.
            if (Design.IsDesignMode)
                return;

            settings = new(notifier);

            if(File.Exists("settings.json"))
                JsonConvert.PopulateObject(File.ReadAllText("settings.json"), settings);

            var initialize_notifier_task = new Task(async () =>
            {
                await notifier.authUser(settings.UseTokenFile ? settings.TokenFile : null, false);
                await notifier.loadConfiguration(settings.ConfigFileName);
                notifier.PollInterval = settings.PollInterval;
                notifier.StreamerNotify += notifyUser;
                notifier.startNotify();
            });

            initialize_notifier_task.Start();

            ToastNotificationManagerCompat.OnActivated += toast_args =>
            {
                ToastArguments args = ToastArguments.Parse(toast_args.Argument);

                Process.Start(new ProcessStartInfo() { FileName = $"https://www.twitch.tv/{args["streamer"]}", UseShellExecute = true });
            };

            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Exit += (s, e) => onExit();
            }

            initialize_notifier_task.Wait();
        }

        /// <summary>
        /// gets called on application exit, used for handeling cleanup of the toast notifications, and for saving the current streamer notification configurations.
        /// </summary>
        private void onExit()
        {
            ToastNotificationManagerCompat.History.Clear();
            ToastNotificationManagerCompat.Uninstall();
            notifier.saveConfiguration(settings.ConfigFileName);
            notifier.stopNotify();
            settings.save();
        }



        /// <summary>
        /// shutsdown the entire application, and removes the app from the tray.
        /// </summary>
        public void exitCommand()
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.TryShutdown();
                onExit();
            }
        }

        public void editorWindowCommand()
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow.Show();
        }

        public void notifyUser(object? sender, IStreamerInfo streamer_info)
        {
            new ToastContentBuilder().
                AddText($"{streamer_info.Streamer.DisplayName} Just started streaming {streamer_info.CurrentCategory?.Name ?? ""}!").
                AddAppLogoOverride(new(streamer_info.Streamer.IconFileOnline), ToastGenericAppLogoCrop.Circle).
                AddAttributionText("Click to go to stream").
                AddArgument("streamer", streamer_info.Streamer.LoginName).
                Show();
        }

        public TwitchNotify notifier = new("mjnfz52170tvwmq4nk1vldg0hufjfv");
    }
}
