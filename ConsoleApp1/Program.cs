using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;
using TwitchLib;
using TwitchLib.Client.Models;
using TwitchLib.Api;
using TwitchLib.Api.Auth;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Interfaces;
using IdentityModel;
using System.Web;
using System.Collections.Specialized;
using System.Net;
using System.Diagnostics;
using Twats;

TwatNotifier notifier = new("mjnfz52170tvwmq4nk1vldg0hufjfv");

await notifier.authUser("token.txt", false);

Console.WriteLine(notifier.addStreamerByName("dougdougw"));


//new ToastContentBuilder().AddText("{STREAMER NAME} Just started streaming {GAME NAME}!").AddAppLogoOverride(new($"{Environment.CurrentDirectory}/11fc4351ff82c494-profile_image-70x70.jpeg"), ToastGenericAppLogoCrop.Circle).AddAttributionText("Click to go to stream").AddAudio(new()).Show();

//TwitchAPI tapi = new();
//tapi.Settings.ClientId = "mjnfz52170tvwmq4nk1vldg0hufjfv";
//tapi.Settings.AccessToken = getUserToken("mjnfz52170tvwmq4nk1vldg0hufjfv");

//var resul_2 = await tapi.Helix.Users.GetUsersAsync(logins: new List<string> { "dougdougw"});
//var result = await tapi.Helix.Streams.GetStreamsAsync(userIds: new List<string> { resul_2.Users[0].Id });

//for (int i = 0; i < result.Streams.Length; i++)
//    Console.WriteLine(result.Streams[i].Title);


//string? getUserToken(string client_id)
//{

    
//}

