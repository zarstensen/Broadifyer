﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using System.Collections;
using ReactiveUI;

namespace Broadifyer.Models
{
    /// <summary>
    /// represents an identifier class that represents a twitch broadcaster
    /// </summary>
    public interface IStreamer : IEqualityComparer<IStreamer>, IComparable<IStreamer>
    {
        /// <summary>
        /// id of the broadcaster
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// display name of the broadcaster.
        /// this will be what the user will see, when searching for the broadcaster, and when viewing their profile page.
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// currently the displayname without capitalization.
        /// </summary>

        [JsonIgnore]
        public string LoginName { get; }
        /// <summary>
        /// path to where the broadcaster icon / profile image should be stored.
        /// </summary>

        [JsonIgnore]
        public string IconFileOnline { get; }
        [JsonIgnore]
        public string IconFileOffline { get; }
        /// <summary>
        /// uri to where the broadcaster icon / profile image is stored online.
        /// </summary>
        public string IconUri { get; }

        /// <summary>
        /// Compares by DisplayName property.
        /// </summary>
        int IComparable<IStreamer>.CompareTo(IStreamer? other)
        {
            if(other == null)
                return 1;

            return DisplayName.CompareTo(other.DisplayName);
        }

        /// <summary>
        /// only the id is compared
        /// </summary>
        bool IEqualityComparer<IStreamer>.Equals(IStreamer? x, IStreamer? y)
        {
            if (x == null || y == null)
                return false;

            return x.Id == y.Id;
        }

        /// <summary>
        /// id of hash is returned
        /// </summary>
        int IEqualityComparer<IStreamer>.GetHashCode(IStreamer obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    /// <summary>
    /// enum passed in a StreamerChanged event.
    /// specifies if the broadcast status (went online or offline) or if the broadcast category changed.
    /// </summary>
    public enum StreamerChange
    {
        Broadcast,
        Category,
    }

    /// <summary>
    /// class representing various configuration and state info about a specific IStreamer instance.
    /// </summary>

    public interface IStreamerInfo : IComparable<IStreamerInfo>
    {
        /// <summary>
        /// the IStreamer instance this class contains data about.
        /// </summary>
        public IStreamer Streamer { get; }
        
        /// <summary>
        /// grayscale version of RgbIcon.
        /// is only guaranteed to be avaliable, if RgbIcon is not null.
        /// </summary>
        [JsonIgnore]
        public Bitmap? GrayIcon { get; }
        
        /// <summary>
        /// the streamer icon stored at IconURI in full RGB colors.
        /// is only guaranteed to be a valid instance if prepareIcons has been called.
        /// </summary>
        [JsonIgnore]
        public Bitmap? RgbIcon { get; }
        /// <summary>
        /// 
        /// dictionary of category ids and their associated ICategoryInfo instances, for this streamer.
        /// 
        /// what categories should be filtered, when deciding whether to send a notification or not.
        /// 
        /// whether the notification will be sent if the broadcaster is streaming or not streaming the category,
        /// is dependent on the WhitelistCategories property.
        /// 
        /// </summary>
        public Dictionary<string, ICategoryInfo> FilteredCategories { get; }
        /// <summary>
        /// if true:
        ///     the user will only be sent a notification, if the streamer is streaming any of the filtered categories
        /// if false:
        ///     notification is sent if streamer is NOT streaming any of the filtered categories.
        /// </summary>
        public bool WhitelistCategories { get; set; }
        /// <summary>
        /// ICategory instance that the broadcaster is currently streaming.
        /// </summary>
        [JsonIgnore]
        public ICategory? CurrentCategory { get; }
        /// <summary>
        /// whether the broadcaster is currently live.
        /// if null, the streamer broadcast status has not yet been polled.
        /// </summary>
        [JsonIgnore]
        public bool? IsLive { get; }
        /// <summary>
        /// specifies whether the user has already been notified about the current broadcaster going live.
        /// ideally, the user should only be notified once, per broadcast start.
        /// </summary>
        [JsonIgnore]
        public bool WasNotified { get; }

        /// <summary>
        /// time of the last attempt to notify the user about a broadcaster going live.
        /// should be used for filtering out incorrect broadcast offline events, that appear right as the broadcaster goes live.
        /// </summary>
        [JsonIgnore]
        public DateTime NotifiedTime { get; }
        /// <summary>
        /// if true:
        ///     notifications will be sent, if the live and filtered categories conditions are met.
        /// if false:
        ///     no notifications will be sent, no matter what
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// should be called, when the streamer icons should be downloaded, preprocessed and prepared for loading into the model.
        /// </summary>
        /// <returns></returns>
        public Task prepareIcons();

        /// <summary>
        /// called any time the streamer state is updated.
        /// StreamerChange specifies which property has changed.
        /// Can either be a Category change, or a broadcast status (is the boradcaster live or not) change.
        /// </summary>
        public event EventHandler<StreamerChange>? StreamerUpdated;

        /// <summary>
        /// 
        /// Compares two IStreamerInfo instances.
        /// 
        /// priority:
        /// is live -> DisplayName
        /// 
        /// meaning live streamers will appear first in a sorted list of IStreamerInfo instances,
        /// afterwhich they are sorted by their DisplayName.
        /// 
        /// </summary>
        int IComparable<IStreamerInfo>.CompareTo(IStreamerInfo? other)
        {
            if (other == null)
                return 1;

            int live_compare = (IsLive ?? false).CompareTo((other.IsLive ?? false));

            if (live_compare != 0)
                return -live_compare;

            return Streamer.CompareTo(other.Streamer);
        }
    }
}
