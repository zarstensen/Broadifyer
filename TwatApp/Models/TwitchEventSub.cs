using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using TwitchLib;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.EventSub;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace EventSub
{
    /// <summary>
    /// represents a subscription that can be subscribed to, using the twitch event sub websockets.
    /// 
    /// stores the subscription type and the subscription conditions.
    /// 
    /// </summary>
    public class Subscription : IEquatable<Subscription>
    {
        public Subscription(string type, Dictionary<string, string> conditions)
        {
            Type = type;
            Conditions = conditions;
        }

        public string Type { get; }
        public Dictionary<string, string> Conditions { get; }

        public bool Equals(Subscription? other)
        {
            if (other == null)
                return false;

            return Type == other.Type && Conditions.SequenceEqual(other.Conditions);
        }
    }

    /// <summary>
    /// class passed to callback function when an event is recieved
    /// 
    /// stores the initial subscription, as well as the message id and event data.
    /// 
    /// </summary>

    public class NotifyEventArgs
    {
        public NotifyEventArgs(string id, Dictionary<string, string> event_data, Subscription subscription)
        {
            this.subscription = subscription;
            this.id = id;
            this.event_data = event_data;
        }

        public Subscription subscription;
        public string id;
        public Dictionary<string, string> event_data;
    }

    /// <summary>
    /// main class for setting up and handling twitch EventSub notifications, using websockets.
    /// 
    /// use subscribe to determine which events will be subscribed to, on the next connect call.
    /// unsubscribe to remove any subscribed events from the subscription list.
    /// 
    /// use connect to activate notification handling, and to update the currently subscribed to events.
    /// 
    /// </summary>
    public class TwitchEventSub
    {
        /// <summary>
        /// constant Uri pointing to the eventsub endpoint
        /// </summary>
        public static readonly Uri TWITCH_ENDPOINT = new Uri("wss://eventsub-beta.wss.twitch.tv/ws");

        /// <summary>
        /// TwitchEventSub constructor
        /// 
        /// takes an TwitchAPI instance, that will be used for any twitch api calls.
        /// needs to have a valid client id and user token already initialized.
        /// 
        /// </summary>
        public TwitchEventSub(TwitchAPI api)
        {
            m_twitch_api = api;
        }

        /// <summary>
        /// delegate for a subscription event callback
        /// </summary>
        /// <param name="args"> a NotifyEventArgs initialized with the relevant data from the recieved notification. </param>
        public delegate void NotifyEvent(NotifyEventArgs args);

        /// <summary>
        /// specifies a subscription to subscribe to, as well as the callback method to be called, when a notification has been recieved
        /// </summary>
        public void subscribe(Subscription subscription, NotifyEvent callback) => m_subscriptions[subscription] = callback;

        /// <summary>
        /// removes a subscription from the subscription list.
        /// Will only stop recieving notifications when reconnected.
        /// </summary>
        public void unsubscripe(Subscription subscription) => m_subscriptions.Remove(subscription!);

        /// <summary>
        /// establish connection to twitch websocket server and sets up all the necessary subscriptions.
        /// 
        /// if called while already connected, a reconnect is performed.
        /// 
        /// </summary>
        /// <returns> dictionary of subscriptions, where each key value pair specifies wether the connection was successfully setup </returns>
        public async Task<Dictionary<Subscription, bool>> connect()
        {
            // disconnect if already connected, performs a reconnect.
            if (web_socket.State == WebSocketState.Open)
                await disconnect();

            Dictionary<Subscription, bool> result = new();

            // establish connection
            await web_socket.ConnectAsync(TWITCH_ENDPOINT, default);

            JsonNode? welcome_payload = JsonNode.Parse(await recvStr());

            m_session_id = welcome_payload?["payload"]?["session"]?["id"]?.GetValue<string>();

            // subscribe to required events

            foreach (Subscription subscription in m_subscriptions.Keys)
            {
                // use twitchlib to subscribe to event.
                CreateEventSubSubscriptionResponse response = await m_twitch_api.Helix.EventSub.CreateEventSubSubscriptionAsync(
                    subscription.Type, "1", new(subscription.Conditions), TwitchLib.Api.Core.Enums.EventSubTransportMethod.Websocket, websocketSessionId: m_session_id);

                // subscriptions list will only contain at maximum 1 element.
                // if there are 0 elements in the list, the subscription was not setup correctly.

                result[subscription] = response.Subscriptions.Length == 1;
            }

            // begin listening for incomming messages on a seperate thread.

            m_websocket_thread = new Thread(socketThread);
            m_websocket_thread.Start();

            return result;
        }

        /// <summary>
        /// disconnects from the setup twitch socket.
        /// 
        /// only does something, if a connection has already been established.
        /// 
        /// </summary>
        public async Task disconnect()
        {
            if (web_socket.State == WebSocketState.Open)
            {
                await web_socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, default);

                // stop and join the separate message thread.
                m_websocket_thread?.Join();
            }
        }

        /// <summary>
        /// recieve a string message from the websocket server.
        /// </summary>
        /// <param name="buffer_size"> buffersize to use when recieving message. </param>
        /// <returns></returns>
        protected async Task<string> recvStr(int buffer_size = 1024)
        {
            // TODO: double check if this works if buffer size is too small.
            byte[] buffer = new byte[buffer_size];
            var result = await web_socket.ReceiveAsync(buffer, default);

            StringBuilder string_result = new(Encoding.UTF8.GetString(buffer, 0, result.Count));

            // if the message was not recieved completely, recieve until all bytes have been stored.

            while (!result.EndOfMessage)
            {
                result = await web_socket.ReceiveAsync(buffer, default);
                string_result.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            }

            return string_result.ToString();
        }

        /// <summary>
        /// function recieves messages in an infinite loop (unless connection is closed), and calls callback functions. 
        /// </summary>
        protected async void socketThread()
        {
            while(true)
            {
                string payload = await recvStr();

                if (payload != string.Empty)
                {
                    Console.WriteLine(payload);
                    JsonNode body = JsonNode.Parse(payload)!;

                    // handle twitch message

                        Trace.WriteLine(body);
                    if (body?["metadata"]?["message_type"]?.GetValue<string>() == "session_keepalive")
                    {
                        // TODO: make sure keepalive is checked
                    }
                    else
                    {

                        Subscription subscription = new Subscription(
                            body!["metadata"]!["subscription_type"]!.GetValue<string>(),
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(body["payload"]!["subscription"]!["condition"]!.ToString())!);

                        NotifyEvent notification_callback = m_subscriptions.First(x => x.Key.Equals(subscription)).Value;

                        await Task.Run(() => notification_callback.Invoke(new NotifyEventArgs(body["metadata"]!["message_id"]!.GetValue<string>(),
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(body["payload"]!["event"]!.ToString())!,
                            subscription)));
                    }
                }
                else
                    break;
            }
        }

        protected void callbackFinished(IAsyncResult ar)
        {
            Console.WriteLine(JsonConvert.SerializeObject(ar));
        }

        ~TwitchEventSub()
        {
            web_socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed connection", default);
        }

        protected TwitchAPI m_twitch_api = new();
        protected ClientWebSocket web_socket = new();
        protected HttpClient m_client = new();
        protected string? m_session_id;
        protected string? m_user_id;
        // map between subscription and its id.
        // the id will only be set, if there is currently a connection where the subscription is active.
        protected Dictionary<Subscription, NotifyEvent> m_subscriptions = new();
        Thread? m_websocket_thread;
    }
}
