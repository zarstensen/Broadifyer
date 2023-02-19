﻿using Avalonia.Media.Imaging;
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
        public IStreamer Streamer { get => streamer_info.Streamer; }
        public Expose<bool, IStreamerInfo> Enable { get; set; }
        public bool IsLive { get => streamer_info.IsLive ?? false; }
        public string DisplayName { get => streamer_info.Streamer.DisplayName; }
        public string CategoryName { get => streamer_info.CurrentCategory?.Name ?? ""; }
        public Bitmap? Icon { get => IsLive ? streamer_info.RgbIcon : streamer_info.GrayIcon; }

        public StreamerViewModel(IStreamerInfo streamer_info)
        {
            this.streamer_info = streamer_info;
         
            Enable = new(streamer_info, nameof(streamer_info.Enable));
        }

        protected React<string> m_category_name = new();

        public IStreamerInfo streamer_info;
    }
}