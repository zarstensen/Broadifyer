using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwatApp.Models;

namespace TwatApp.ViewModels
{
    public class StreamerViewModel : ReactiveObject
    {
        public IStreamer Streamer { get => m_streamer_info.Streamer; }
        public Expose<bool, IStreamerInfo> Enable { get; set; }
        public bool IsLive { get => m_streamer_info.IsLive ?? false; }
        public string DisplayName { get => m_streamer_info.Streamer.DisplayName; }
        public string CategoryName { get => m_streamer_info.CurrentCategory?.Name ?? ""; }
        public Bitmap? Icon { get => IsLive ? m_streamer_info.RgbIcon : m_streamer_info.GrayIcon; }

        public StreamerViewModel(IStreamerInfo streamer_info)
        {
            m_streamer_info = streamer_info;
         
            Enable = new(m_streamer_info, nameof(m_streamer_info.Enable));
        }

        protected React<string> m_category_name = new();

        public IStreamerInfo m_streamer_info;
    }
}
