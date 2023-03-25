using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Broadifyer.Models;
using TwitchLib.Api.Helix.Models.Bits;

namespace Broadifyer.ViewModels
{

    public class Setting : ReactiveObject
    {
        public Setting(string name, string description, SettingValueBase setting_value)
        {
            Name = name;
            Description = description;
            Value = setting_value;
        }

        public string Name { get; set; }
        public string Description { get; set; }

        public virtual SettingValueBase Value { get; set; }
        public object Data { get => Value.Data; }
    }

    public abstract class SettingValueBase : ReactiveObject
    {
        public SettingValueBase(object init_value)
        {
            Data = init_value;
        }

        public object Data { get; protected set; }
    }

    public abstract class SettingValue<TData, TView> : SettingValueBase
        where TData : notnull
        where TView : notnull
    {
        protected SettingValue(TData init_value) : base(init_value) {}

        public TView ViewValue { get => GetSetting((TData)Data); set { Data = SetSetting(value); this.RaisePropertyChanged(nameof(ViewValue)); } }

        public abstract TView GetSetting(TData data);
        public abstract TData SetSetting(TView view_data);
    }

    public class NumericSetting : SettingValue<int, string>
    {
        public NumericSetting(int init_value) : base(init_value) {}

        public override string GetSetting(int data)
        {
            return data.ToString();
        }

        public override int SetSetting(string view_data)
        {
            if (!int.TryParse(view_data, out int res) || res <= 0)
                throw new DataValidationException("Must Be A Non Zero Positive Number!");

            return res;
        }
    }

    public class StringSetting : SettingValue<string, string>
    {
        public StringSetting(string init_value) : base(init_value) {}
        public override string GetSetting(string data) => data;
        public override string SetSetting(string view_data) => view_data;
    }

    public class PathSetting : SettingValue<string, string>
    {
        public PathSetting(string init_value) : base(init_value)
        {
        }

        public async Task folderDialouge()
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                OpenFolderDialog dialog = new OpenFolderDialog();
                var res = await dialog.ShowAsync(desktop.MainWindow);

                if (res == null)
                    return;

                ViewValue = res;
            }
        }

        public override string GetSetting(string data) => data;

        public override string SetSetting(string view_data)
        {
            string relative_path = Path.GetRelativePath(Environment.CurrentDirectory, view_data);

            if (!relative_path.Contains("..\\"))
                return relative_path;
            else
                return view_data;
        }
    }

    public class FileSetting : SettingValue<string, string>
    {
        public FileSetting(string init_value) : base(init_value)
        {
        }

        public async Task fileDialouge()
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                var res = await dialog.ShowAsync(desktop.MainWindow);

                if (res == null)
                    return;

                ViewValue = res[0];
            }
        }

        public override string GetSetting(string data) => data;

        public override string SetSetting(string view_data)
        {
            string relative_path = Path.GetRelativePath(Environment.CurrentDirectory, view_data);

            if (!relative_path.Contains("..\\"))
                return relative_path;
            else
                return view_data;
        }
    }

    public class ToggleSetting : SettingValue<bool, bool>
    {
        public ToggleSetting(bool init_value) : base(init_value) {}

        public override bool GetSetting(bool data) => data;

        public override bool SetSetting(bool view_data) => view_data;
    }

    public class SettingsViewModel : ViewModelBase
    {
        Setting RunOnStartupSetting { get; set; }
        Setting PollIntervalSetting { get; set; }
        Setting UrgentNotificationsSetting { get; set; }
        Setting BroadcastTimeoutSetting { get; set; }
        Setting ConfigFileSetting { get; set; }
#if WIN64
        Setting UseIntegratedBrowserSetting { get; set; }
#endif
        Setting UseTokenFileSetting { get; set; }
        Setting ClientIDSetting { get; set; }
        Setting RedirectURISetting { get; set; }

        public ReactiveCommand<Unit, Unit> Exit { get; set; }

        public SettingsViewModel(TwitchNotify notifier)
        {
            m_notifier = notifier;

            // common settings

            RunOnStartupSetting = new("Run On Startup",
                "Start the application in a minimized state, every time the computer boots",
                new ToggleSetting(AppVM.Settings.RunsOnStartup));

            PollIntervalSetting = new("Poll Interval",
                "How often the program checks if a streamer has gone live.",
                new NumericSetting(AppVM.Settings.PollInterval));

            UrgentNotificationsSetting = new("Use Important Notifications",
                "Notifications will be marked as important, which will allow the notification to appear, whilst using an application in fullscreen mode.",
                new ToggleSetting(AppVM.Settings.UseUrgentNotifications));

            // advanced settings

            BroadcastTimeoutSetting = new("New Broadcast Timeout",
                "How many seconds the broadcaster must have been offline, before a new broadcast will result in an alert.",
                new NumericSetting(AppVM.Settings.NewBroadcastTimeout));

            ConfigFileSetting = new("Config File",
                "Name of the config file, used to store the current streamer configurations.",
                new FileSetting(AppVM.Settings.ConfigFileName));

#if WIN64
            UseIntegratedBrowserSetting = new("Integrated Browser",
                "Use an integrated browser window, instead of the default browser when opening urls.",
                new ToggleSetting(AppVM.Settings.UseIntegratedBrowser));
#endif

            UseTokenFileSetting = new("Use Token File",
                "Store the twitch api locally, in order to avoid opening a browser tab, every time the app is started. see README for further info.",
                new ToggleSetting(AppVM.Settings.UseTokenFile));

            ClientIDSetting = new Setting("Client ID",
                "The twitch api client id, that is used when polling the twitch api.",
                new StringSetting(AppVM.Settings.SetupSettings.ClientID));

            RedirectURISetting = new Setting("Redirect URI / URL",
                "The redirect uri / url, that will be used when going through the implicit grant auth flow.",
                new StringSetting(AppVM.Settings.SetupSettings.RedirectURI));

            Exit = ReactiveCommand.Create(() => {

                if(AppVM.Settings.ConfigFileName != (string)ConfigFileSetting.Data
                || AppVM.Settings.UseTokenFile != (bool)UseTokenFileSetting.Data
                || AppVM.Settings.SetupSettings.ClientID != (string) ClientIDSetting.Data
                || AppVM.Settings.SetupSettings.RedirectURI != (string) RedirectURISetting.Data)
                    WindowVM?.showInfo("Changes will take effect once the app has been restarted.", 5000);

                AppVM.Settings.RunsOnStartup = (bool) RunOnStartupSetting.Data;
                AppVM.Settings.PollInterval = (int)PollIntervalSetting.Data;
                AppVM.Settings.UseUrgentNotifications = (bool) UrgentNotificationsSetting.Data;
                AppVM.Settings.NewBroadcastTimeout = (int) BroadcastTimeoutSetting.Data;
                AppVM.Settings.ConfigFileName = (string) ConfigFileSetting.Data;

#if WIN64
                AppVM.Settings.UseIntegratedBrowser = (bool) UseIntegratedBrowserSetting.Data;
#endif

                AppVM.Settings.UseTokenFile = (bool) UseTokenFileSetting.Data;
                AppVM.Settings.SetupSettings.ClientID = (string) ClientIDSetting.Data;
                AppVM.Settings.SetupSettings.RedirectURI = (string) RedirectURISetting.Data;
                AppVM.Settings.save();
            });
        }

        protected TwitchNotify m_notifier;
    }
}
