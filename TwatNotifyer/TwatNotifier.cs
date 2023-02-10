using System.Diagnostics;
using System.Net;
using System.Web;
using System.Xml.Linq;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api.Interfaces;

namespace Twats
{
        public async static Game? fromName(TwitchAPI api, string name)
        {
            

        }


    public class TwatNotifier
    {

        public TwatNotifier(string client_id)
        {
            twitch_api.Settings.ClientId = m_client_id;
        }

        public async Task<bool> addStreamerByName(string name)
        {
            // get streamer id, if streamer exists

            GetUsersResponse users = await twitch_api.Helix.Users.GetUsersAsync(logins: new List<string> { name });


            if (users.Users.Length != 1)
                // do multiple users witht he same name exist?
                return false;

            

            return await addStreamer(users.Users[0].Id);
        }

        public async Task<bool> addStreamer(string id)
        {
            // check if streamer exists
            GetUsersResponse users = await twitch_api.Helix.Users.GetUsersAsync(ids: new List<string> { id });

            if (users.Users.Length != 1)
                return false;

            m_streamers.Add(id);

            return true;
        }

        public async Task<bool> authUser(string token_file_dir, bool force_verify)
        {
            string token = "";

            if(File.Exists(token_file_dir))
                token = File.ReadAllText(token_file_dir);

            // TODO: expand check if token is valid

            if(token.Length < 10)
            {
                HttpClient client = new();

                UriBuilder auth_endpoint = new("https://id.twitch.tv/oauth2/authorize");

                var query = HttpUtility.ParseQueryString("");

                query["client_id"] = twitch_api.Settings.ClientId;
                query["scope"] = "user:read:follows";
                query["response_type"] = "token";
                query["redirect_uri"] = "http://localhost:3000/";
                query["force_verify"] = force_verify.ToString();

                auth_endpoint.Query = query.ToString();

                Console.WriteLine(auth_endpoint.Uri);
                HttpRequestMessage msg = new(HttpMethod.Get, auth_endpoint.Uri);

                Process.Start(new ProcessStartInfo() { FileName = auth_endpoint.Uri.ToString(), UseShellExecute = true });

                HttpListener listener = new();
                listener.Prefixes.Add(query["redirect_uri"]);
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
                        token = req.QueryString["access_token"];
                        var resp = File.ReadAllBytes("close_tab.html");
                        res.OutputStream.Write(resp);

                        res.Close();
                        break;
                    }
                }

                File.WriteAllText(token_file_dir, token);
            }

            twitch_api.Settings.AccessToken = token;

            // update user id

            GetUsersResponse current_user = await twitch_api.Helix.Users.GetUsersAsync();

            m_user_id = current_user.Users[0].Id;

            return true;
        }

        public async Task<Game?> gameFromName(string name)
        {
            GetGamesResponse games_response = await twitch_api.Helix.Games.GetGamesAsync(gameNames: new List<string> { name });

            if (games_response.Games.Length != 1)
                return null;

            return games_response.Games[0];
        }

        public async Task<Game?> gameFromId(string id)
        {
            GetGamesResponse games_response = await twitch_api.Helix.Games.GetGamesAsync(gameIds: new List<string> { id });

            if (games_response.Games.Length != 1)
                return null;

            return games_response.Games[0];
        }

        protected string m_client_id = "mjnfz52170tvwmq4nk1vldg0hufjfv";
        protected string m_user_id;
        TwitchAPI twitch_api = new();
        protected List<string> m_streamers = new();
    }
}