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
using Windows.Data.Xml.Dom;
using Broadifyer.Models;
using TwitchLib.Api.Helix;
using Windows.UI.Notifications;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Broadifyer.ViewModels;
using Broadifyer.Views;
using Avalonia.Threading;

#if WIN64
using WebViewControl;
#endif

namespace Broadifyer.ViewModels
{
    /// <summary>
    /// datastructure storing various configurable settings for the rest of the app.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// inner settings datastrucutre, containing settings used for initializing the TwitchNotify instance.
        /// 
        /// some settings, like PollInterval, needs to have a TwitchNotify instance to store the value,
        /// however, the TwitchNotify instance cannot be created whilst deserializing the json file into the settings instance.
        /// 
        /// therefore the specific settings needed for constructing the TwitchNotify instance, is placed inside this inner class,
        /// allowing the program to first deserialize this specific datastructure,create the TwitchNotify instance, and finally load the rest of the setting.
        /// 
        /// </summary>
        public class Setup
        {
            /// <summary>
            /// what twitch api client id, the TwitchNotify instance should use, when sending api requests.
            /// </summary>
            public string ClientID { get; set; } = "mjnfz52170tvwmq4nk1vldg0hufjfv";
            /// <summary>
            /// the uri that will be redirected to, when the user must be authorized.
            /// also needs to be registered in the twitch api client app.
            /// </summary>
            public string RedirectURI { get; set; } = "http://localhost:3000/";
        }

        /// <summary>
        /// property containing settings needed when constructing the TwitchNotify instance.
        /// </summary>
        public Setup SetupSettings { get; set; } = new();

        /// <summary>
        /// commandline command used to start the program, when it is run on computer boot.
        /// </summary>
        [JsonIgnore]
        public string StartupPath = $"\"{Environment.ProcessPath}\" --minimized";
        /// <summary>
        /// the file this datastructure will be serialized in to.
        /// </summary>
        [JsonIgnore]
        public string SettingsFile = "settings.json";
        /// <summary>
        /// sets whether the app will be started, when the computer boots.
        /// 
        /// the registry key 'Run', is used to control this behaviour.
        /// 
        /// </summary>
        public bool RunsOnStartup
        {
            get
            {
                RegistryKey? run_key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (run_key == null)
                    return false;

                return (string?)run_key.GetValue("Broadifyer") == StartupPath;
            }
            set
            {
                if (RunsOnStartup == value)
                    return;

                RegistryKey? run_key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (!value)
                    run_key!.DeleteValue("Broadifyer");
                else
                    run_key!.SetValue("Broadifyer", StartupPath!);
            }
        }

        /// <summary>
        /// sets where the currently used app configuration file will be stored.
        /// 
        /// when updated, the current config file will be renamed to the new file name.
        /// 
        /// </summary>
        public string ConfigFileName {
            get => m_config_file_name;
            set
            {
                if (File.Exists(m_config_file_name) && m_config_file_name != value)
                    File.Copy(m_config_file_name, value, true);

                m_config_file_name = value;
            }
        }

        /// <summary>
        /// sets whether the TwitchNotify instance should store the user token locally,
        /// in order to avoid opening a browser tab, every time the user needs to be reauthorized.
        /// </summary>
        public bool UseTokenFile { get => m_use_token_file;
            set
            {
                if (!value && File.Exists(TokenFile))
                    File.Delete(TokenFile);

                m_use_token_file = value;
            }
        }

#if WIN64
        /// <summary>
        /// specifies whether to use the default browser to open urls, or use an integrated browser, utilizing chrome.
        /// </summary>
        public bool UseIntegratedBrowser { get; set; } = true;
#endif
        public Settings() => m_notifier = new(SetupSettings.ClientID, SetupSettings.RedirectURI);

        [JsonIgnore]
        public string? TokenFile { get => UseTokenFile ? "token.txt" : null; }
        /// <summary>
        /// see TwitchNotify.PollInterval
        /// </summary>
        public int PollInterval { get => m_notifier.PollInterval; set => m_notifier.PollInterval = value; }
        /// <summary>
        /// see TwitchNotify.NewBroadcastTimeout
        /// </summary>
        public int NewBroadcastTimeout { get => m_notifier.NewBroadcastTimeout; set => m_notifier.NewBroadcastTimeout = value; }

        /// <summary>
        /// specifies whether the windows toast notifications will be marked as urgent,
        /// allowing them to show up whilst in full screen mode.
        /// </summary>
        public bool UseUrgentNotifications { get; set; } = false;

        /// <summary>
        /// loads the serialized settings from the settings file, and returns a TwitchNotify instance, initialized with the SetSettings properties.
        /// 
        /// any time load is called, the previously returned TwitchNotify instance should stop its poll thread,
        /// and the new TwitchNotify isntance should be used isntead.
        /// 
        /// </summary>
        public TwitchNotify load()
        {
            if (File.Exists(SettingsFile))
            {
                var setup_settings_json = JsonNode.Parse(File.ReadAllText(SettingsFile))?[nameof(SetupSettings)];

                if (setup_settings_json != null)
                    JsonConvert.PopulateObject(setup_settings_json.ToJsonString(), SetupSettings);

                m_notifier = new TwitchNotify(SetupSettings.ClientID, SetupSettings.RedirectURI);

                JsonConvert.PopulateObject(File.ReadAllText(SettingsFile), this);
            }
            
            return m_notifier;
        }

        /// <summary>
        /// serialize current settings instance to SettingsFile.
        /// </summary>
        public void save()
        {
            File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        protected TwitchNotify m_notifier;
        protected bool m_use_token_file = false;
        protected string m_config_file_name = "config.json";
    }
    
    public class VersionNumber : IComparable<VersionNumber>
    {
        public VersionNumber(int major = 0, int minor = 0, int build = 0, int revision = 0)
            : this(new int[] { major, minor, build, revision }) { }

        public VersionNumber(int[] versions)
        {
            m_versions = new int[4];

            Array.Copy(versions, m_versions, versions.Length);
        }

        public int[] Version { get => m_versions; }

        public int Major { get => m_versions[0]; }
        public int Minor { get => m_versions[0]; }
        public int Build { get => m_versions[0]; }
        public int Revision { get => m_versions[0]; }

        int[] m_versions;

        public int CompareTo(VersionNumber? other)
        {
            if (other == null)
                return 1;

            for(int i = 0; i < m_versions.Length; i++)
                if (Version[i] != other.Version[i])
                    return Version[i].CompareTo(other.Version[i]);

            return 0;
        }

        public override string ToString()
        {
            return string.Join('.', Version);
        }
    }

    public class AppViewModel : ViewModelBase
    {
        // event though it is initialized through settings.load(), the compiler still complains, so do this hack to disable the warning
        public TwitchNotify notifier = default!;
        public Settings Settings { get; protected set; } = new();

        /// <summary>
        /// returns a version string with the given format
        /// Version: x.x.x.x
        /// </summary>
        public string VersionString
        {
            get => $"Version: {Version}";
        }

        /// <summary>
        /// List containing
        /// </summary>
        public VersionNumber Version { get; protected set; }

        /// <summary>
        /// tooltip displayed when hovering over the tray icon.
        /// </summary>
        public string ToolTipText { get; set; }

        /// <summary>
        /// event raised when the TwitchNotify instance has finished authorizing the user, loaded all saved configurations and begun notifying.
        /// </summary>
        public Action<AppViewModel>? NotifierInitialized;

        /// <summary>
        /// initialize the notifier instance, and set up the required event handles.
        /// </summary>
        public AppViewModel()
        {
            // set webview control settings

#if WIN64
            WebView.Settings.OsrEnabled = false;
            WebView.Settings.LogFile = "BrowserLog.txt";
            WebView.Settings.PersistCache = true;
            WebView.Settings.CachePath = $"{Environment.CurrentDirectory}/Cache/";
#endif

            // initialize VersionNumber

            string? location;

            Assembly assembly = Assembly.GetExecutingAssembly();

            // if packaged in a single file, the process path must be used, instead of the assembly location.

            if (assembly.Location != string.Empty)
                location = assembly.Location;
            else
                location = Environment.ProcessPath;

            FileVersionInfo file_version_info = FileVersionInfo.GetVersionInfo(location ?? string.Empty);

            Version = new(file_version_info.ProductMajorPart, file_version_info.ProductMinorPart, file_version_info.ProductBuildPart, file_version_info.ProductPrivatePart);

            ToolTipText = $"Broadifyer\n{VersionString}";

            // this code should not be called, if in design mode, as neither a notification nor a twitch api call will be made, during design mode.
            if (Design.IsDesignMode)
                return;

            notifier = Settings.load();
            notifier.OpenUri += openUri;

            Trace.WriteLine(Settings.TokenFile);

            var notify_init_task = new Task(async () =>
            {
                // attempt to authorize
                // if a BadScopeException is thrown, the credentials in the token file are outdated or invalid.
                // therefore a new authorization token is recieved, by deleting the current token file, and calling authUser again.
                try
                {
                    await notifier.authUser(Settings.TokenFile, false);
                }
                catch (TwitchLib.Api.Core.Exceptions.BadScopeException e)
                {
                    Trace.WriteLine(e);

                    if (Settings.UseTokenFile)
                        File.Delete(Settings.TokenFile!);

                    await notifier.authUser(Settings.TokenFile, false);
                }

                await notifier.loadConfiguration(Settings.ConfigFileName);
                notifier.PollInterval = Settings.PollInterval;
                notifier.StreamerNotify += notifyUser;
                notifier.startNotify();

                NotifierInitialized?.Invoke(this);
            });

            var fault_task = notify_init_task.ContinueWith(t => Trace.WriteLine(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            
            notify_init_task.Start();

            // called when the toast notification is clicked on.
            ToastNotificationManagerCompat.OnActivated += toast_args =>
            {
                // the streamer argument, will contain the login name of the streamer.
                // redirect to twitch.tv/streamer name, in order to open their stream.
                ToastArguments args = ToastArguments.Parse(toast_args.Argument);

                Process.Start(new ProcessStartInfo() { FileName = $"https://www.twitch.tv/{args["streamer"]}", UseShellExecute = true });
            };

            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Exit += (s, e) => onExit();
        }

        // opens the passed uri, using either the default browser, or an integrated browser, depending on the UseIntegratedBrowser setting.
        private void openUri(object? s, Uri uri)
        {
#if WIN64
            if (Settings.UseIntegratedBrowser)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    AuthBrowserWindow auth_window = new()
                    {
                        DataContext = new AuthBrowserViewModel(uri)
                    };

                    auth_window.Show();

                    NotifierInitialized += _ => Dispatcher.UIThread.InvokeAsync(() => auth_window.Close());
                });
            }
            else
#endif
                Process.Start(new ProcessStartInfo() { FileName = uri.ToString(), UseShellExecute = true });
        }

        /// <summary>
        /// gets called on application exit, used for handeling cleanup of the toast notifications, and for saving the current streamer notification configurations.
        /// </summary>
        private void onExit()
        {
            ToastNotificationManagerCompat.History.Clear();
            ToastNotificationManagerCompat.Uninstall();
            notifier.saveConfiguration(Settings.ConfigFileName);
            notifier.stopNotify();
            Settings.save();
        }

        /// <summary>
        /// shutsdown the entire application, and removes the app from the tray.
        /// </summary>
        public void exitCommand()
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.TryShutdown();

                // the desktop.Exit event is only invoked, if the user closed the application,
                // so it is called here manually, in order to perform cleanup.
                onExit();
            }
        }

        /// <summary>
        /// open up the configuration editor window.
        /// </summary>
        public void editorWindowCommand()
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow.Show();
        }

        /// <summary>
        /// send a toast notification displaying the IStreamerInfos streamer name, and broadcast category.
        /// 
        /// the toast notification scenario is controlled by the settings.UseUrgentNotifications setting.
        /// 
        /// </summary>
        public void notifyUser(object? sender, IStreamerInfo streamer_info)
        {
            // the ToastNotificationBuilder class does not allow setting the scenario to urgen,
            // and the AppNotificationBuilder requires the program to have access to the windows app sdk,
            // so an xml document is used instead, to send the toast notificaiton.
            var notifier = ToastNotificationManagerCompat.CreateToastNotifier();

            XmlDocument doc = new();
            doc.LoadXml(File.ReadAllText($"{AppContext.BaseDirectory}/Assets/Notification.xml"));

            XmlElement toast = (XmlElement)doc.SelectSingleNode("/toast");

            toast.SetAttribute("launch", $"streamer={streamer_info.Streamer.LoginName}");
            string scenario = Settings.UseUrgentNotifications ? "urgent" : "default";
            toast.SetAttribute("scenario", scenario);

            XmlElement content = (XmlElement)doc.SelectSingleNode("/toast/visual/binding/text");

            content.InnerText = $"{streamer_info.Streamer.DisplayName} Just started streaming {streamer_info.CurrentCategory?.Name ?? ""}!";

            XmlElement icon = (XmlElement)doc.SelectSingleNode("/toast/visual/binding/image");

            icon.SetAttribute("src", streamer_info.Streamer.IconFileOnline);

            notifier.Show(new ToastNotification(doc));
        }
    }
}
