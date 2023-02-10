using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api;
using System.IO;

namespace TwatApp.Models
{
    class TwitchNotify
    {
        public TwitchNotify(string client_id)
        {
            m_twitch_api.Settings.ClientId = client_id;
            m_event_sub = new(m_twitch_api);
        }

        public async Task<string?> streamerIdFromName(string name)
        {
            GetUsersResponse users = await m_twitch_api.Helix.Users.GetUsersAsync(logins: new List<string> { name });


            if (users.Users.Length != 1)
                // do multiple users witht he same name exist?
                return null;

            return users.Users[0].Id;
        }

        public async Task<bool> authUser(string token_file_dir, bool force_verify)
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
                query["scope"] = "user:read:follows";
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

            m_twitch_api.Settings.AccessToken = token;

            // update user id

            GetUsersResponse current_user = await m_twitch_api.Helix.Users.GetUsersAsync();

            m_user_id = current_user.Users[0].Id;

            return true;
        }


        public EventSub.TwitchEventSub m_event_sub;
        protected TwitchAPI m_twitch_api = new();
        protected string? m_user_id;
    }
}
