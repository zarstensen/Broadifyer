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
using TwatApp.Models;
using TwitchLib.Api.Helix.Models.Bits;

namespace TwatApp.ViewModels
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
    {
        protected SettingValue(TData init_value) : base(init_value)
        {
        }

        public TView ViewValue { get => GetSetting((TData)Data); set { Data = SetSetting(value); this.RaisePropertyChanged(nameof(ViewValue)); } }

        public abstract TView GetSetting(TData data);
        public abstract TData SetSetting(TView view_data);
    }

    public class NumericSetting : SettingValue<int, string>
    {
        public NumericSetting(int init_value) : base(init_value)
        {
        }

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

    public class PathSetting : SettingValue<string, string>
    {
        public PathSetting(string init_value) : base(init_value)
        {
        }

        public async void folderDialouge()
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

        public async void fileDialouge()
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
        Setting UseTokenFileSetting { get; set; }

        public ReactiveCommand<Unit, Unit> Exit { get; set; }

        public SettingsViewModel(TwitchNotify notifier)
        {
            m_notifier = notifier;

            // common settings

            RunOnStartupSetting = new("Run On Startup",
                "Start the application in a minimized state, every time the computer boots",
                new ToggleSetting(AppVM.settings.RunsOnStartup));

            PollIntervalSetting = new("Poll Interval",
                "How many seconds to wait, before checking if the broadcaster is live.",
                new NumericSetting(AppVM.settings.PollInterval));

            UrgentNotificationsSetting = new("Use Urgent Notifications",
                "Notifications will be marked as urgent, which will allow the notification to appear, whilst using an application in fullscreen mode.",
                new ToggleSetting(AppVM.settings.UseUrgentNotifications));

            // advanced settings

            BroadcastTimeoutSetting = new("New Broadcast Timeout",
                "How many seconds the broadcaster must have been offline, before a new broadcast will result in an alert.",
                new NumericSetting(AppVM.settings.NewBroadcastTimeout));

            ConfigFileSetting = new("Config File",
                "Name of the config file, used to store the current streamer configurations.",
                new FileSetting(AppVM.settings.ConfigFileName));

            UseTokenFileSetting = new("Use Token File",
                "Store the twitch api locally, in order to avoid opening a browser window, every time the app is started.\nIncreases risk of abuse from 3rd party applications.",
                new ToggleSetting(AppVM.settings.UseTokenFile));

            Exit = ReactiveCommand.Create(() => {

                if(AppVM.settings.ConfigFileName != (string)ConfigFileSetting.Data || AppVM.settings.UseTokenFile != (bool)UseTokenFileSetting.Data)
                    WindowVM?.showInfo("Changes will take effect once the app has been restarted.", 5000);

                AppVM.settings.RunsOnStartup = (bool) RunOnStartupSetting.Data;
                AppVM.settings.PollInterval = (int)PollIntervalSetting.Data;
                AppVM.settings.UseUrgentNotifications = (bool) UrgentNotificationsSetting.Data;
                AppVM.settings.NewBroadcastTimeout = (int) BroadcastTimeoutSetting.Data;
                AppVM.settings.ConfigFileName = (string) ConfigFileSetting.Data;
                AppVM.settings.UseTokenFile = (bool) UseTokenFileSetting.Data;
                AppVM.settings.save();
            });
        }

        protected TwitchNotify m_notifier;
    }
}
