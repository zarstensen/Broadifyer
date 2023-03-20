using Avalonia.Controls;
using Avalonia.Threading;
using Broadifyer.ViewModels;
using Broadifyer.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Broadifyer.ViewModels
{
    public class UpdateViewModel : ViewModelBase
    {
        public React<bool> IsLoading { get; set; } = true;
        public React<bool> FoundNewRelease { get; set; } = false;
        public React<bool> FoundNoNewRelease { get; set; } = false;
        public React<string> PopupText { get; set; } = "";

        public UpdateViewModel()
        {
            PopupText.Value = "Checking for new versions";
            m_http_client.DefaultRequestHeaders.Add("User-Agent", "agent");
            Trace.WriteLine(AppVM);

            Dispatcher.UIThread.Post(async () =>
            {
                var (new_version, version_number) = await checkNewVersion();

                IsLoading.Value = false;

                if (!new_version ?? false)
                {
                    FoundNoNewRelease.Value = true;
                    PopupText.Value = "Already up to date!";
                }
                else
                {
                    FoundNewRelease.Value = true;
                    PopupText.Value = $"Version {version_number} is avaliable to install!";
                }
            });

            
        }

        /// <summary>
        /// checks if there exists a newer release version in the github repository.
        /// 
        /// returns null if the check failed.
        /// 
        /// </summary>
        public async Task<(bool?, VersionNumber?)> checkNewVersion()
        {
            Regex version_regex = new(@"^v(\d).(\d).(\d)$");

            HttpRequestMessage msg = new(HttpMethod.Get, "https://api.github.com/repos/karstensensensen/Broadifyer/releases/latest");
            var response = await m_http_client.SendAsync(msg);

            string? version_str = JsonNode.Parse(await response.Content.ReadAsStringAsync())?["tag_name"]?.ToString();

            if (version_str == null)
            {
                await WindowVM.showInfo("Unable to retrieve latest version!", 5000);
                return (null, null);
            }

            var version_parsed = version_regex.Match(version_str).Groups.Values.Skip(1).Select(x => int.Parse(x.Value)).ToArray();

            VersionNumber version_number = new(version_parsed);

            return (AppVM.Version.CompareTo(version_number) < 0, version_number);
        }

        public async Task installRelease()
        {
            FoundNewRelease.Value = false;
            IsLoading.Value = true;
            PopupText.Value = "Downloading latest release";

            await downloadLatestRelease("Release.zip");
            installRelease("Release.zip");
        }

        /// <summary>
        /// 
        /// downloads the latest github release, to the passed destination file.
        /// 
        /// </summary>
        public async Task downloadLatestRelease(string dest)
        {
            // download the new release, different depending on the current binary architecture.

            string suffix;

            switch (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture)
            {
                case System.Runtime.InteropServices.Architecture.X64:
                    suffix = "x64";
                    break;
                case System.Runtime.InteropServices.Architecture.X86:
                    suffix = "x86";
                    break;
                default:
                    await WindowVM.showInfo("Unable to find supported release for current architecture!", 5000);
                    return;
            }

            var release_stream = await m_http_client.GetStreamAsync($"https://github.com/karstensensensen/Broadifyer/releases/latest/download/Broadifyer-{suffix}.zip");
            FileStream file_stream = new(dest, FileMode.OpenOrCreate);


            await release_stream.CopyToAsync(file_stream);

            file_stream.Close();
        }

        public void installRelease(string release)
        {
            Process.Start($"Updater", $"{Process.GetCurrentProcess().Id} \"{release}\"");
            AppVM.exitCommand();
        }

        protected HttpClient m_http_client = new();

    }
}
