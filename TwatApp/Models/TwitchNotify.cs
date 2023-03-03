﻿using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TwitchLib.Api;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.Extensions.ReleasedExtensions;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using SystemStream = System.IO.Stream;
using TwitchLibStream = TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream;

namespace TwatApp.Models
{
    /// <summary>
    /// class responsible for notifying a user, using windows toast notifications, based on registered streamers and their category conditions.
    ///
    /// use addStreamers and removeStreamrs to register or unregister streamers for notifications.
    /// streamersFromIds or streamersFromNames can be used for retrieving IStreamer instances, from various broadcaster identifiers.
    ///
    /// same for categoriesFromIds and categoriesFromNames.
    ///
    /// use startNotify and stopNotify to control when an instance will send toast notifications.
    ///
    /// </summary>
    public class TwitchNotify
    {
        #region public

        /// <summary>
        /// how often the twitch api will be polled in seconds.
        /// if the value is too low, rate limit exceptions may occur.
        /// </summary>
        public int PollInterval { get; set; } = 60;

        /// <summary>
        /// how many seconds should pass, before a begin broadcast event will be seen as valid.
        /// this is used to filter out any incorrect broadcast offline events shortly after a broadcaster goes live.
        /// </summary>
        public double NewBroadcastTimeout { get; set; } = 30;

        /// <summary>
        /// invoked whenever a user should be notified of a streamers broadcast.
        /// </summary>
        public EventHandler<IStreamerInfo>? StreamerNotify;

        /// <summary>
        /// readonly dictionary of the registered streamers, mapping the streamer id to the corresponding IStreamerInfo instance
        /// </summary>
        public ReadOnlyDictionary<string, IStreamerInfo> Streamers { get => new(m_streamers); }

        /// <summary>
        /// construct TwitchNotify instance with the passed twitch client id.
        /// this id will be used when making calls to the twitch api.
        /// </summary>
        public TwitchNotify(string client_id)
        {
            m_twitch_api.Settings.ClientId = client_id;
        }

        /// <summary>
        /// start the poll thread, and activate notifying of streamers going live.
        ///
        /// must be called after user has been authorized.
        ///
        /// </summary>
        public void startNotify()
        {
            m_polling = true;

            m_poll_thread = new Thread(async () => await pollThread());
            m_poll_thread.Start();
        }

        /// <summary>
        /// stop notifying when a streamer goes live.
        /// </summary>

        public void stopNotify()
        {
            m_polling = false;

            m_poll_thread?.Join();
            m_poll_thread = null;
        }

        /// <summary>
        /// authenticate current user using an implicit OAuth2 flow.
        /// optionally store the retrieved token in a file, in order to avoid opening up a tab in the default browser, every time the program is run
        /// </summary>
        /// <param name="token_file_dir"> specify the name of the file containing a token. If null, no token file will be used. </param>
        /// <param name="force_verify"> specfifies whether to force the user to reverify themselves, even if they have already been verified before. </param>
        /// <returns></returns>
        public async Task<bool> authUser(string? token_file_dir, bool force_verify)
        {
            string token = "";

            if (File.Exists(token_file_dir))
                token = File.ReadAllText(token_file_dir);

            // TODO: expand check if token is valid

            if (token.Length < 10)
            {
                HttpClient client = new();

                UriBuilder auth_endpoint = new("https://id.twitch.tv/oauth2/authorize");

                var query = HttpUtility.ParseQueryString("");

                query["client_id"] = m_twitch_api.Settings.ClientId;
                query["scope"] = "user:read:follows channel:moderate";
                query["response_type"] = "token";
                query["redirect_uri"] = "http://localhost:3000/";
                query["force_verify"] = force_verify.ToString();

                auth_endpoint.Query = query.ToString();

                Console.WriteLine(auth_endpoint.Uri);
                HttpRequestMessage msg = new(HttpMethod.Get, auth_endpoint.Uri);

                HttpResponseMessage rmsg = await client.SendAsync(msg);
                Trace.WriteLine(await rmsg.Content.ReadAsStringAsync());

                Process.Start(new ProcessStartInfo() { FileName = auth_endpoint.Uri.ToString(), UseShellExecute = true });

                HttpListener listener = new();
                listener.Prefixes.Add(query["redirect_uri"]!);
                listener.Start();

                while (true)
                {
                    var context = listener.GetContext();

                    var req = context.Request;
                    var res = context.Response;
                    res.ContentType = "text/html";

                    if (req.QueryString["error"] != null)
                    {
                        return false;
                    }
                    else if (req.QueryString.Count == 0)
                    {
                        var resp = File.ReadAllBytes("parse_token.html");
                        res.OutputStream.Write(resp);

                        res.Close();
                    }
                    else
                    {
                        token = req.QueryString["access_token"]!;
                        var resp = File.ReadAllBytes("close_tab.html");
                        res.OutputStream.Write(resp);

                        res.Close();
                        break;
                    }
                }

                if (token_file_dir != null)
                    File.WriteAllText(token_file_dir, token);
            }

            m_twitch_api.Settings.AccessToken = token;

            // update user id

            GetUsersResponse current_user = await m_twitch_api.Helix.Users.GetUsersAsync();

            m_user_id = current_user.Users[0].Id;

            return true;
        }

        /// <summary>
        /// get a list of streamers from the specified streamer names.
        /// </summary>
        /// <param name="names"> list of names to convert to IStreamer instances </param>
        public async Task<List<IStreamer>> streamersFromNames(List<string> names)
        {
            if (names.Count == 0)
                return new();

            GetUsersResponse users = await m_twitch_api.Helix.Users.GetUsersAsync(logins: names);

            return streamersFromResponse(users);
        }

        /// <summary>
        /// attempts to retrieve a single IStreamer object from the given name.
        /// if the streamer is not found, null is returned.
        /// </summary>
        public async Task<IStreamer?> streamerFromName(string name)
        {
            var streamers = await streamersFromNames(new() { name });

            if (streamers.Count != 1)
                return null;

            return streamers[0];
        }

        /// <summary>
        /// get a list of streamers form the specified streamer ids
        /// </summary>
        public async Task<List<IStreamer>> streamersFromIds(List<string> ids)
        {
            // if an ids list of length 0 is passed to GetUsersAsync, it will assume no ids have been passed,
            // and then return 20 twitch users based on some random property, which is not what we want,
            // so we check if the ids count is 0 here, in order to avoid this problem.
            if (ids.Count == 0)
                return new();

            GetUsersResponse users = await m_twitch_api.Helix.Users.GetUsersAsync(ids: ids);

            return streamersFromResponse(users);
        }

        /// <summary>
        /// attempts to retrieve a single IStreamer object from the given id.
        /// if the streamer is not found, null is returned.
        /// </summary>
        public async Task<IStreamer?> streamerFromId(string id)
        {
            var streamers = await streamersFromIds(new() { id });

            if (streamers.Count != 1)
                return null;

            return streamers[0];
        }

        /// <summary>
        /// get a list of all the streamers the verified user is currently following.
        /// </summary>
        public async Task<List<IStreamer>> followedStreamers()
        {
            GetUsersFollowsResponse response = await m_twitch_api.Helix.Users.GetUsersFollowsAsync(fromId: m_user_id, first: 100);

            List<string> ids = new((int)response.TotalFollows);

            foreach (Follow follow in response.Follows)
                ids.Add(follow.ToUserId);

            return await streamersFromIds(ids);
        }

        /// <summary>
        /// get category instances from category names.
        ///
        /// makes use of the same cache strategy as the categoriesFromIds method.
        /// is way less efficient, as the name needs to be searches for in the cache using a linear method,
        /// so please use the categoriesFromIds if the category id is known.
        ///
        /// </summary>
        public async Task<List<ICategory>> categoriesFromNames(List<string> names)
        {
            names = names.Select(elem => elem.ToLower()).ToList();

            List<string> non_cached_names = names.Where(name => !m_cached_categories.Values.Any(value => value.Name == name)).ToList();

            if (non_cached_names.Count > 0)
            {
                GetGamesResponse categories = await m_twitch_api.Helix.Games.GetGamesAsync(gameNames: non_cached_names);

                List<ICategory> new_categories = categoriesFromResponse(categories);

                // add new categories to the cache

                foreach (ICategory new_category in new_categories)
                    m_cached_categories[new_category.Id] = new_category;
            }

            // now act as if all ids exist in the cache

            return m_cached_categories.Values.Where(category => names.Contains(category.Name.ToLower())).ToList();
        }

        public async Task<ICategory?> categoryFromName(string name)
        {
            List<ICategory> found_categoreis = await categoriesFromNames(new() { name });

            if (found_categoreis.Count != 1)
                return null;
            return found_categoreis[0];
        }

        /// <summary>
        /// get category instances from category ids.
        ///
        /// additionally, if the category has already been retrieved,
        /// this method returns a cached ICategory instance, instead of performing a get request to the twitch api-
        /// </summary>
        public async Task<List<ICategory>> categoriesFromIds(List<string> ids)
        {
            List<string> non_cached_ids = ids.Where(id => !m_cached_categories.ContainsKey(id)).ToList();

            if (non_cached_ids.Count > 0)
            {
                GetGamesResponse categories = await m_twitch_api.Helix.Games.GetGamesAsync(gameIds: ids);

                List<ICategory> new_categories = categoriesFromResponse(categories);

                // add new categories to the cache

                foreach (ICategory new_category in new_categories)
                    m_cached_categories[new_category.Id] = new_category;
            }

            // now act as if all ids exist in the cache

            return m_cached_categories.Values.Where(category => ids.Contains(category.Id)).ToList();
        }

        /// <summary>
        /// add the passed streamers to the streamer list and associate a default IStreamerInfo instance to the streamer.
        /// if the streamer already exists in the list, nothing is done.
        /// </summary>
        public async Task addStreamers(List<IStreamer> streamers)
        {
            foreach (IStreamer streamer in streamers)
            {
                if (!m_streamers.ContainsKey(streamer.Id))
                {
                    m_streamers[streamer.Id] = new StreamerInfo((Streamer)streamer);
                    await m_streamers[streamer.Id].prepareIcons();
                }
            }
            await poll();
        }

        /// <summary>
        /// remove the streamer from the streamer list.
        ///
        /// this means the IStreamInfo instance is also forgotten, so if the streamer is later readded, the categories and so on will not be remembered.
        ///
        /// if the intention is to temporarily stop notifications from the passed streamers,
        /// please use the associated IStreamerInfos Enable property isntead.
        /// </summary>
        public void removeStreamers(List<IStreamer> streamers)
        {
            foreach (IStreamer streamer in streamers)
            {
                m_streamers.Remove(streamer.Id);
                File.Delete(streamer.IconFileOnline);
                File.Delete(streamer.IconFileOffline);
            }
        }

        /// <summary>
        /// adds a category to filter for notifications associated with the passed streamer.
        /// </summary>
        public ICategoryInfo filterCategory(ICategory category, IStreamer streamer)
        {
            m_streamers[streamer.Id].FilteredCategories.Add(category.Id, new CategoryInfo(category as Category));
            return m_streamers[streamer.Id].FilteredCategories[category.Id];
        }

        /// <summary>
        /// save the currently registered streamers, and their correspoinding info, to a specified json file, that can later be loaded.
        /// </summary>
        /// <param name="file"></param>
        public void saveConfiguration(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(m_streamers, Formatting.Indented));
        }

        /// <summary>
        /// loads a previously saved configuration from the passed json file.
        /// overwrites any currently registered streamers.
        /// </summary>
        /// <param name="file"></param>
        public async Task loadConfiguration(string file)
        {
            if (File.Exists(file))
            {
                string contents = File.ReadAllText(file);

                var config = JsonConvert.DeserializeObject<Dictionary<string, StreamerInfo>>(contents)!;
                
                m_streamers = config?.Select(kv => new KeyValuePair<string, IStreamerInfo>(kv.Key, kv.Value)).ToDictionary(kv => kv.Key, kv => kv.Value) ?? new();

                foreach (IStreamerInfo streamer in currentStreamers())
                {
                    await streamer.prepareIcons();
                    
                    foreach (ICategoryInfo category in streamer.FilteredCategories.Values)
                        await category.prepareIcons();
                }
            }
        }

        /// <summary>
        /// retrieve a list of all the currently registered streamers info instances.
        /// </summary>
        public List<IStreamerInfo> currentStreamers()
        {
            return m_streamers.Values.ToList();
        }

        #endregion public

        #region protected fields

        protected TwitchAPI m_twitch_api = new();
        protected string? m_user_id;
        protected HttpClient m_http_client = new();
        protected Dictionary<string, IStreamerInfo> m_streamers = new();
        protected bool m_polling = false;
        protected Thread? m_poll_thread = null;
        protected Dictionary<string, ICategory> m_cached_categories = new();

        #endregion protected fields

        #region protected methods

        // construct a list of IStreamer objecst, based on the Users property in the passed GetUserResponse instance.
        protected List<IStreamer> streamersFromResponse(GetUsersResponse users)
        {
            List<IStreamer> streamers = new(users.Users.Length);

            foreach (User user in users.Users)
                streamers.Add(new Streamer(user.Id, user.DisplayName, user.ProfileImageUrl));

            return streamers;
        }

        // construct a list of ICategory objects, based on the Game property in the passed GetGamesResponse instance.
        protected List<ICategory> categoriesFromResponse(GetGamesResponse games)
        {
            List<ICategory> categories = new(games.Games.Length);

            foreach (Game game in games.Games)
                categories.Add(new Category(game.Id, game.Name, game.BoxArtUrl));

            return categories;
        }

        // thread responsible for polling the twitch api
        protected async Task pollThread()
        {
            while (m_polling)
            {
                await poll();

                Thread.Sleep(PollInterval * 1000);
            }
        }

        protected async Task poll()
        {
            try
            {
                // if there are no registered live streamers, simply do nothing and wait for the poll interval,

                List<string> ids = currentStreamers().Where(info => info.Enable).Select(info => info.Streamer.Id).ToList();

                if (ids.Count == 0)
                    return;
                
                GetStreamsResponse response = await m_twitch_api.Helix.Streams.GetStreamsAsync(userIds: ids);

                HashSet<string> live_users = new();

                // handle users that went live

                foreach (TwitchLibStream stream in response.Streams)
                {
                    IStreamerInfo streamer_info = m_streamers[stream.UserId];

                    live_users.Add(stream.UserId);

                    if (streamer_info.Streamer.DisplayName == "zarstensen")
                        Trace.WriteLine(stream.Type);

                    await updateStreamInfo(streamer_info, stream.Type == "live", stream.GameId);
                }

                // handle users that are not / no longer live

                foreach (IStreamerInfo streamer_info in m_streamers.Values.Where(info => !live_users.Contains(info.Streamer.Id)))
                    await updateStreamInfo(streamer_info, false, null);
            }
            catch (Exception e) when (e is GatewayTimeoutException || e is TooManyRequestsException || e is InternalServerErrorException)
            {
                Trace.WriteLine($"Exception caught whilst polling twitch api:\n{e.GetType().Name}\n{e}");
            }
        }

        // update the passed streamer_info api and call StreamerChanged and streamerNotify if necessary
        protected async Task updateStreamInfo(IStreamerInfo streamer_info, bool is_live, string? category_id)
        {

            // check if a toast noficiation should be sent
            // this should happen if the user has just gone live, and the filtered categories are correct.
            //
            // the first time this is called, the IsLive property will be null.
            // currently, the user will not get a notification, when a streamer is polled for the first time.
            
            bool broadcast_change;

            if ((is_live && (DateTime.Now - streamer_info.NotifiedTime).TotalSeconds > NewBroadcastTimeout) || !is_live)
                broadcast_change = (streamer_info.IsLive ?? true) != is_live;
            else
                broadcast_change = false;


            bool category_change = broadcast_change && (streamer_info.CurrentCategory?.Id ?? String.Empty) != category_id;


            bool should_notify = false;
                        
            if (broadcast_change || category_change)
            {
                if (streamer_info.FilteredCategories.Count == 0)
                {
                    should_notify = is_live;
                }
                else
                {
                    should_notify = !streamer_info.WhitelistCategories;

                    foreach (ICategoryInfo category_info in streamer_info.FilteredCategories.Values)
                    {
                        if (category_info.Category.Id == category_id)
                        {
                            if (!category_info.Enable)
                                should_notify = !should_notify;

                            break;
                        }
                    }
                }
            }


            // update fields.

            if (streamer_info is StreamerInfo sinfo)
            {
                sinfo!.is_live = is_live;

                // reset notified status, as the user should only be notified once per broadcast start.
                if (!(sinfo.IsLive ?? false))
                    sinfo.was_notified = false;

                if (category_id != null && category_id != "")
                    sinfo.current_category = (await categoriesFromIds(new() { category_id }))[0];
                else
                    sinfo.current_category = null;

                if (broadcast_change)
                    sinfo.notifyUpdate(StreamerChange.Broadcast);
                else if (category_change)
                    sinfo.notifyUpdate(StreamerChange.Category);

                if (should_notify && !sinfo.was_notified)
                {
                    sinfo.was_notified = true;
                    StreamerNotify?.Invoke(this, streamer_info);
                    sinfo.notified_time = DateTime.Now;
                }
                
            }
            else
                // should never be hit.
                throw new InvalidCastException();

        }

        #endregion protected methods

        #region interface implementations

        private class Streamer : IStreamer
        {
            public string Id => m_id;
            public string DisplayName => m_name;
            public string LoginName { get => DisplayName.ToLower(); }

            public string IconUri => m_icon_uri;
            public string IconFileOnline => Path.GetFullPath($"icons/streamers/{Id}.jpg");
            public string IconFileOffline => Path.GetFullPath($"icons/streamers/{Id}-offline.jpg");

            public Streamer(string id, string displayname, string iconuri)
            {
                m_id = id;
                m_name = displayname;
                m_icon_uri = iconuri;
            }

            public async Task prepareIcon()
            {
                // prepare a full rgb local image, as well as a grayscale version of the image,
                // in order to avoid re applying the grayscale image step, every time the offline icon is used.
                
                if (File.Exists(IconFileOnline))
                    return;

                string full_icon_online_path = Path.GetFullPath(IconFileOnline);
                string full_icon_offline_path = Path.GetFullPath(IconFileOffline);

                Directory.CreateDirectory(Directory.GetParent(full_icon_online_path)!.ToString());
                await File.WriteAllBytesAsync(full_icon_online_path, await s_http_client.GetByteArrayAsync(IconUri));

                Bitmap rgb_img = new(full_icon_online_path);
                Bitmap gray = grayFromRgb(rgb_img);

                gray.Save(full_icon_offline_path);

            }

            protected string m_name;
            protected string m_id;
            protected string m_icon_uri;
            protected static HttpClient s_http_client = new();

            protected Bitmap grayFromRgb(Bitmap source)
            {
                // in order to modify the source image, it is saved to a memory stream, which is loaded into a WiteableBitmap, that then is modified and returned as a Bitmap.

                SystemStream image_stream = new MemoryStream();

                source.Save(image_stream);

                image_stream.Seek(0, SeekOrigin.Begin);

                WriteableBitmap grayscale_bitmap = WriteableBitmap.Decode(image_stream);

                using (var buffer = grayscale_bitmap.Lock()) unsafe
                    {
                        byte* pixel = (byte*)buffer.Address;

                        for (int i = 0; i < buffer.Size.Width * buffer.Size.Height; i++)
                        {
                            if (buffer.Format == PixelFormat.Rgba8888 || buffer.Format == PixelFormat.Bgra8888)
                            {
                                // take the average value of the red green and blue channel, as the gray value.
                                byte avg = (byte)((pixel[0] + pixel[1] + pixel[2]) / 3);
                                pixel[0] = avg;
                                pixel[1] = avg;
                                pixel[2] = avg;
                                pixel += 4;
                            }
                        }
                    }

                return grayscale_bitmap;
            }
        }

        private class StreamerInfo : IStreamerInfo
        {
            public StreamerInfo(Streamer streamer, Dictionary<string, CategoryInfo>? filteredcategories = null, bool whitelist = true, bool enable = true)
            {
                this.streamer = streamer;
                m_categories = filteredcategories?.ToDictionary(x => x.Key, x => (ICategoryInfo)x.Value) ?? new();
                m_whitelist = whitelist;
                m_enable = enable;
            }

            public IStreamer Streamer => streamer;

            public Dictionary<string, ICategoryInfo> FilteredCategories => m_categories;
            public bool WhitelistCategories { get => m_whitelist; set => m_whitelist = value; }
            public bool Enable { get => m_enable; set => m_enable = value; }

            public ICategory? CurrentCategory => current_category;

            public bool? IsLive => is_live;

            public Bitmap? GrayIcon => m_icon_gray;

            public Bitmap? RgbIcon => m_icon_rgb;

            public bool WasNotified => was_notified;

            [JsonIgnore]
            public DateTime NotifiedTime => notified_time;

            [JsonIgnore]
            public Streamer streamer;

            [JsonIgnore]
            public ICategory? current_category = null;

            [JsonIgnore]
            public bool? is_live = null;

            [JsonIgnore]
            public bool was_notified = false;
            [JsonIgnore]
            public DateTime notified_time = new();

            public event EventHandler<StreamerChange>? StreamerUpdated;

            public async Task prepareIcons()
            {
                await streamer.prepareIcon();

                m_icon_rgb = new(Streamer.IconFileOnline);
                m_icon_gray = new(Streamer.IconFileOffline);
            }
            
            // StreamerUpdate cannot be invoked outside this class, so this helper method is implemented,
            // in order for the TwitchNotifyer instance, to invoke a StreamerUpdated event.
            public void notifyUpdate(StreamerChange change)
            {
                StreamerUpdated?.Invoke(this, change);
            }
            
            [JsonIgnore]
            protected Dictionary<string, ICategoryInfo> m_categories = new();

            [JsonIgnore]
            protected bool m_whitelist = true;

            [JsonIgnore]
            protected bool m_enable = true;

            [JsonIgnore]
            protected Bitmap? m_icon_rgb = null;

            [JsonIgnore]
            protected Bitmap? m_icon_gray = null;
        }

        public class Category : ICategory
        {
            public string Name => m_name;

            public string Id => m_id;

            public string IconFile => Path.GetFullPath($"icons/categories/{Id}.png");

            public string IconUri => m_icon_uri.Replace("{width}", "300").Replace("{height}", "400");

            public Category(string id, string name, string iconuri)
            {
                m_id = id;
                m_name = name;
                m_icon_uri = iconuri;
            }

            protected string m_name;
            protected string m_id;
            protected string m_icon_uri;
        }

        public class CategoryInfo : ICategoryInfo
        {
            public CategoryInfo(Category category, bool enable = true)
            {
                m_category = category;
                m_enable = enable;
            }

            public ICategory Category => m_category;

            public bool Enable { get => m_enable; set => m_enable = value; }

            public Bitmap? Icon => m_icon;

            public async Task prepareIcons()
            {
                // icon has already been downloaded, do nothing.
                if (!File.Exists(Category.IconFile))
                {
                    string full_icon_path = Path.GetFullPath(Category.IconFile);
                    Directory.CreateDirectory(Directory.GetParent(full_icon_path)!.ToString());
                    await File.WriteAllBytesAsync(full_icon_path, await s_http_client.GetByteArrayAsync(Category.IconUri));
                }

                m_icon = new Bitmap(Category.IconFile);

            }

            protected static HttpClient s_http_client = new();

            protected static Dictionary<ICategory, Bitmap?> s_cached_bitmaps = new();
            protected Bitmap? m_icon = null;
            protected bool m_enable;
            protected ICategory m_category;
        }

        #endregion interface implementations
    }
}