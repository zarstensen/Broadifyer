
using EventSub;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

TwitchEventSub sub = new("mjnfz52170tvwmq4nk1vldg0hufjfv", new TwitchLib.Api.TwitchAPI());

await sub.authUser("token.txt", false);

sub.subscribe("stream.online", new Dictionary<string, string>
{
    { "broadcaster_user_id", await sub.streamerIdFromName("andersonjph") !},
}, StreamBeginCallback);



sub.subscribe("stream.online", new Dictionary<string, string>
{
    { "broadcaster_user_id", await sub.streamerIdFromName("zarstensen") !},
}, StreamBeginCallback);

var result = await sub.connect();

void StreamBeginCallback(NotifyEventArgs args)
{
    Console.WriteLine(args.event_data["broadcaster_user_name"]);
}

Console.WriteLine(JsonConvert.SerializeObject(result));

while(true) { }

//Uri twitch_uri = new("wss://eventsub-beta.wss.twitch.tv/ws");

//using ClientWebSocket ws = new();

//await ws.ConnectAsync(twitch_uri, default);

//var bytes = new byte[1024];
//var result = await ws.ReceiveAsync(bytes, default);
//string res = Encoding.UTF8.GetString(bytes, 0, result.Count);

//await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default);

//Console.WriteLine(res);
