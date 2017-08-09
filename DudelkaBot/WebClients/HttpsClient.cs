using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using DudelkaBot.system;
using DudelkaBot.enums;
using DudelkaBot.Logging;
using System.Text.RegularExpressions;

namespace DudelkaBot.WebClients
{
    public class HttpsClient
    {
        private string id;
        private string token;
        private string url;
        private HttpClient client;
        private WinHttpHandler handler;
        private static Random random = new Random();
        private static string patternrandom = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static string DjURL = "https://twitch-dj.ru/api/get_track/channel_id";
        private static string yidUrl = "https://www.youtube.com/watch?v=";
        private static string StreamUrl = "https://api.twitch.tv/kraken/streams/?channel=name&callback=twitch_info&client_id=";
        private static string ChannelUrl = "https://api.twitch.tv/kraken/users?login=";
        private static string SubscribersUrl = "https://api.twitch.tv/kraken/channels/";
        private static string TwitchDJPageUrl = "https://twitch-dj.ru/c/";
        private static string CountChattersUrl = "http://tmi.twitch.tv/group/user/channel_name/chatters";
        private int lengthid = 30;

        public HttpsClient(string userid, string token, string url)
        {
            id = userid;
            this.token = token;
            this.url = url;

            handler = new WinHttpHandler();
            handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            handler.ClientCertificateOption = ClientCertificateOption.Automatic;
            client = new HttpClient(handler);
        }

        public Tuple<string, string> GetCountChattersAndModerators(string channel_name)
        {
            if (Channel.IrcClient != null)
                Channel.IrcClient.isConnect();
            try
            {
                HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, CountChattersUrl.Replace("channel_name",channel_name));
                m.Method = new HttpMethod("GET");
                m.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/vnd.twitchtv.v5+json"));
                var task = client.SendAsync(m);

                if (task.IsFaulted || !task.Result.IsSuccessStatusCode)
                    return new Tuple<string, string>("error", "error");

                JObject text = JObject.Parse(Encoding.UTF8.GetString(task.Result.Content.ReadAsByteArrayAsync().Result));
                var results = text["chatter_count"];

                var moderators = text["chatters"].Children().First().First.ToList();

                return new Tuple<string, string>(results.ToObject<string>(),moderators.Count.ToString());
            }
            catch
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Logger.ShowLineCommonMessage(ex.Message + ex.Data + ex.StackTrace);
                //if (ex.InnerException != null)
                //{
                //    Logger.ShowLineCommonMessage(ex.InnerException.Message + ex.InnerException.Data + ex.InnerException.StackTrace);
                //}
                //Console.ForegroundColor = ConsoleColor.Gray;
                return new Tuple<string, string>("error", "error");
            }
        }

        private string UniqueMessageId()
        {
            string result = "";
            for (int i = 0; i < lengthid; i++)
            {
                result += patternrandom[random.Next(0, 62)];
            }
            return result;
        }
        [Obsolete("check weather is disabled")]
        public Tuple<string,string> GetWeatherInTown(string townname)
        {
            try
            {
                Channel.IrcClient.isConnect();
                WebRequest request;
                request = WebRequest.Create($"http://www.meteoservice.ru/weather/now/{townname}.html");
                using (var response = request.GetResponseAsync().Result)
                {
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        string data = reader.ReadToEnd();

                        //var match = new Regex(")

                        var match = new Regex(@"<h1>(?<town>.*)</h1>").Match(data);
                        if (!match.Success || !match.Groups["town"].Success)
                            return null;
                        string temp = new Regex(@"<td class=""title"">Температура воздуха:</td>[^<]*?<td>(?<temp>[^<]+)</td>").Match(data).Groups["temp"].Value.Replace("&deg;", "°");
                        

                        string osadki = new Regex(@"<td class=""title"">Облачность:</td>[^<]*?<td>(?<osadki>[^<]+)</td>").Match(data).Groups["osadki"].Value;
                        return new Tuple<string, string>(temp, osadki);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public Tuple<string, DateTime> GetChannelId(string channelname, string client_id)
        {
            if (Channel.IrcClient != null)
                Channel.IrcClient.isConnect();
            try
            {
                HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, ChannelUrl + channelname);
                m.Method = new HttpMethod("GET");
                m.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/vnd.twitchtv.v5+json"));
                m.Headers.Add("Client-ID", client_id);
                var task = client.SendAsync(m);

                if (task.IsFaulted || !task.Result.IsSuccessStatusCode)
                    return null;

                JObject text = JObject.Parse(Encoding.UTF8.GetString(task.Result.Content.ReadAsByteArrayAsync().Result));
                var results = text["users"];

                if (results.Count() == 0)
                    return null;
                results = results.First;
                var _id = results["_id"].ToObject<string>();
                var created_at = results["created_at"].ToObject<DateTime>().ToLocalTime();
                return new Tuple<string, DateTime>(_id,created_at);
            }
            catch
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Logger.ShowLineCommonMessage(ex.Message + ex.Data + ex.StackTrace);
                //if (ex.InnerException != null)
                //{
                //    Logger.ShowLineCommonMessage(ex.InnerException.Message + ex.InnerException.Data + ex.InnerException.StackTrace);
                //}
                //Console.ForegroundColor = ConsoleColor.Gray;
                return null;
            }
        }

        static int TotalMonths(DateTime startDate, DateTime endDate)
        {
            DateTime dt1 = startDate.Date, dt2 = endDate.Date;
            if (dt1 > dt2 || dt1 == dt2) return 0;

            var m = ((dt2.Year - dt1.Year) * 12)
                + dt2.Month - dt1.Month
                + (dt2.Day >= dt1.Day - 1 ? 0 : -1)//поправка на числа
                + ((dt1.Day == 1 && DateTime.DaysInMonth(dt2.Year, dt2.Month) == dt2.Day) ? 1 : 0);//если начальная дата - 1-е число меяца, а конечная - последнее число, добавляется 1 месяц
            return m;
        }

        public Dictionary<string,int> GetChannelSubscribers(string channel_id, string client_id, string token_channel)
        {
            if (Channel.IrcClient != null)
                Channel.IrcClient.isConnect();
            try
            {
                HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, SubscribersUrl + $"{channel_id}/subscriptions");
                m.Method = new HttpMethod("GET");
                m.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/vnd.twitchtv.v5+json"));
                m.Headers.Add("Client-ID", client_id);
                m.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token_channel);
                var task = client.SendAsync(m);
                task.Wait();
                if (task.IsFaulted || !task.Result.IsSuccessStatusCode)
                    return null;

                JObject text = JObject.Parse(Encoding.UTF8.GetString(task.Result.Content.ReadAsByteArrayAsync().Result));
                var results = text["subscriptions"].Children().ToList();

                if (results.Count == 0)
                    return null;
                Dictionary<string, int> list = new Dictionary<string, int>();
                foreach (var item in results)
                {
                    list.Add(item["user"]["name"].ToObject<string>(), TotalMonths(item["created_at"].ToObject<DateTime>(),item["user"]["updated_at"].ToObject<DateTime>()));
                }

                return list;
            }
            catch
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Logger.ShowLineCommonMessage(ex.Message + ex.Data + ex.StackTrace);
                //if (ex.InnerException != null)
                //{
                //    Logger.ShowLineCommonMessage(ex.InnerException.Message + ex.InnerException.Data + ex.InnerException.StackTrace);
                //}
                //Console.ForegroundColor = ConsoleColor.Gray;
                return null;
            }
        }

        public Tuple<Status, int, DateTime> GetChannelInfo(string channelname, string client_id)
        {
            if (Channel.IrcClient != null)
                Channel.IrcClient.isConnect();
            try
            {
                HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, StreamUrl.Replace("name", channelname) + client_id);
                m.Method = new HttpMethod("GET");
                m.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                m.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };
                var task = client.SendAsync(m);

                if (task.IsFaulted || !task.Result.IsSuccessStatusCode)
                    return new Tuple<Status, int, DateTime>(Status.Offline, 0, default(DateTime));

                string te = Encoding.UTF8.GetString(task.Result.Content.ReadAsByteArrayAsync().Result);
                te = te.Substring(16);
                te = te.Remove(te.Length - 1, 1);

                JObject text = JObject.Parse(te);
                var results = text["streams"].Children().ToList();

                if (results.Count == 0)
                    return new Tuple<Status, int, DateTime>(Status.Offline, 0, default(DateTime));

                var stream = results.ElementAtOrDefault(0);
                if (stream == default(JObject))
                    return new Tuple<Status, int, DateTime>(Status.Unknown, 0, default(DateTime));
                var viewers = stream["viewers"].ToObject<int>();
                var created_at = stream["created_at"].ToObject<DateTime>().ToLocalTime();
                return new Tuple<Status, int, DateTime>(Status.Online, viewers, created_at);
            }
            catch
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Logger.ShowLineCommonMessage(ex.Message + ex.Data + ex.StackTrace);
                //if (ex.InnerException != null)
                //{
                //    Logger.ShowLineCommonMessage(ex.InnerException.Message + ex.InnerException.Data + ex.InnerException.StackTrace);
                //}
                //Console.ForegroundColor = ConsoleColor.Gray;
                return new Tuple<Status, int, DateTime>(Status.Unknown, 0, default(DateTime));
            }

        }

        public string GetMusicLinkFromTwitchDJ(string channel_name)
        {
            if (channel_name == "customstories")
                channel_name = "Customstories";
            if (Channel.IrcClient != null)
                Channel.IrcClient.isConnect();

            HttpWebRequest request = WebRequest.CreateHttp(TwitchDJPageUrl + channel_name);
            HttpWebResponse response = (HttpWebResponse) request.GetResponseAsync().Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response != null)
                {
                    readStream = new StreamReader(receiveStream, Encoding.UTF8);
                }

                string data = readStream.ReadToEnd();
                return data;
            }
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel_id"> channel id from twitch-dj.ru/channelname , see back.php GET request url </param>
        public async Task<Tuple<string, string>> GetMusicFromTwitchDJ(string channel_id)
        {
            if (Channel.IrcClient != null)
                Channel.IrcClient.isConnect();
            HttpResponseMessage message;
            try
            {
                HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, DjURL.Replace("channel_id", channel_id));
                m.Method = new HttpMethod("GET");
                m.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                m.Headers.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("gzip"));
                m.Headers.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("deflate"));
                m.Headers.UserAgent.ParseAdd("runscope/0.1");

                //m.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };
                //handler = new WinHttpHandler();
                //handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                //handler.ClientCertificateOption = ClientCertificateOption.Automatic;

                message = await client.SendAsync(m);

                var task = message.Content.ReadAsByteArrayAsync();
                task.Wait();
                string te = Encoding.UTF8.GetString(task.Result);
                if (te == "null" || te == null)
                    return new Tuple<string, string>(string.Empty,string.Empty);
                JObject text = JObject.Parse(te);
                var yid = text["yid"].ToObject<string>();
                var title = text["title"].ToObject<string>();
                var author = text["author"].ToObject<string>();
                var time = text["start_time"].ToObject<string>();
                var math = Regex.Split(time, @"[.:\-]").Select(a => int.Parse(a)).ToArray();
                var startTime = new DateTime(math[2],math[1],math[0],math[3],math[4], 0);
                //var results = text["1"].Children().ToList();
                //string status = results[5].ToObject<string>();

                ////GetMusicLinkFromTwitchDJ("dariya_willis");

                //if (status != "1")
                //    return "Сейчас музыка не играет FeelsBadMan ";
                //string title = results[2].ToObject<string>();
                //string author = results[6].ToObject<string>();
                //DateTime startTime = results[4].ToObject<DateTime>();

                if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(author) && DateTime.Now.Subtract(startTime) < TimeSpan.FromMinutes(20))  
                    return new Tuple<string, string>($"Сейчас играет: {title} Kreygasm Заказал: {author} Kappa ", yidUrl + yid);
                return new Tuple<string, string>(string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage(ex.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
                return new Tuple<string, string>("Ошибка загрузки FeelsBadMan", string.Empty);
            }
        }

        public async void GetChannelStatus(string to_id, string message)
        {
            if (Channel.IrcClient != null)
                Channel.IrcClient.isConnect();
            var msg = new HttpRequestMessage(new HttpMethod("POST"), url);
            msg.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/vnd.twitchtv.v5+json"));
            msg.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token);
            string data = "{" + $"\"on_site\":\"\",\"body\":\"{message}\",\"from_id\":{id},\"to_id\":{to_id},\"nonce\":\"{UniqueMessageId()}\"" + "}";


            msg.Content = new StringContent(data, Encoding.UTF8, "application/json");
            try
            {
                await client.SendAsync(msg);
            }
            catch (WebException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
                return;
            }
        }

        public async void SendPostWhisperMessage(string to_id, string message)
        {
            if (Channel.IrcClient != null)
                Channel.IrcClient.isConnect();
            try
            {
                if (Channel.IrcClient != null)
                    Channel.IrcClient.isConnect();
                var msg = new HttpRequestMessage(new HttpMethod("POST"), url);
                msg.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/vnd.twitchtv.v5+json"));
                msg.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token);
                string data = "{" + $"\"on_site\":\"\",\"body\":\"{message}\",\"from_id\":{id},\"to_id\":{to_id},\"nonce\":\"{UniqueMessageId()}\"" + "}";

                msg.Content = new StringContent(data, Encoding.UTF8, "application/json");
                await client.SendAsync(msg);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.ShowLineCommonMessage("379 " + ex.Message + ex.Data + ex.StackTrace);
                if (ex.InnerException != null)
                    Logger.ShowLineCommonMessage(ex.InnerException.Message + ex.InnerException.Data + ex.InnerException.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
                return;
            }
        }

        internal async Task<string> MakeGetRequest()
        {
            if (Channel.IrcClient != null)
                Channel.IrcClient.isConnect();
            if (string.IsNullOrEmpty(id) && string.IsNullOrWhiteSpace(token) && string.IsNullOrWhiteSpace(token))
                return string.Empty;

            token = token?.ToLower().Replace("oauth:", "");

            // If the URL already has GET parameters, we cannot use the GET parameter initializer '?'
            HttpWebRequest request = url.Contains("?")
                ? (HttpWebRequest)WebRequest.Create(new Uri($"{url}&client_id={id}"))
                : (HttpWebRequest)WebRequest.Create(new Uri($"{url}?client_id={id}"));

            request.Method = "GET";
            request.Accept = $"application/vnd.twitchtv.v5+json";
            request.Headers["Client-ID"] = id;

            if (!string.IsNullOrWhiteSpace(token))
                request.Headers["Authorization"] = $"OAuth {token}";
            else if (!string.IsNullOrEmpty(token))
                request.Headers["Authorization"] = $"OAuth {token}";

            try
            {
                using (var responseStream = await request.GetResponseAsync())
                {
                    return await new StreamReader(responseStream.GetResponseStream(), Encoding.Unicode, true).ReadToEndAsync();
                }
            }
            catch (WebException e)
            {
                handleWebException(e);
                return null;
            }

        }

        internal async Task<string> MakePostRequest(string message, ulong to_userid, string method, string requestData = null, byte[] data = null)
        {
            if (Channel.IrcClient != null)
                Channel.IrcClient.isConnect();
            if (data == null)
                data = new UTF8Encoding().GetBytes(requestData ?? "");
            token = token?.ToLower().Replace("oauth:", "");

            var request = (HttpWebRequest)WebRequest.Create(new Uri($"{url}"));
            request.Method = method;
            request.Accept = $"application/vnd.twitchtv.v5+json";
            request.ContentType = method == "POST"
                ? "application/json" :
            "application/x-www-form-urlencoded";
            //request.Headers["from_id"] = id;
            //request.Headers["to_id"] = to_userid.ToString();
            //request.Headers["nonce"] = "DW6esVLIDJEoD09HE0877023HvrpW";
            //request.Headers["body"] = message;
            //request.Headers["on_site"] = "";
            string d = "{" + $"\"on_site\":\"\",\"body\":\"hi\",\"from_id\":{id},\"to_id\":{to_userid},\"nonce\":\"u2ZOYt3k2mwuCY8Uz99hPKH4umokoT\"" + "}";
            data = Encoding.Unicode.GetBytes(d);
            //StringContent cn = new StringContent(d);

            //await client.PostAsync("https://im.twitch.tv/v1/messages?on_site=",cn,);


            if (!string.IsNullOrWhiteSpace(token) || !string.IsNullOrWhiteSpace(token))
                request.Headers["Authorization"] = $"OAuth {token}";

            using (var requestStream = await request.GetRequestStreamAsync())
            {
                requestStream.Write(data, 0, data.Length);
            }

            try
            {
                using (var responseStream = await request.GetResponseAsync())
                {
                    return await new StreamReader(responseStream.GetResponseStream(), Encoding.Unicode, true).ReadToEndAsync();
                }
            }
            catch (WebException e)
            {
                handleWebException(e);
                return null;
            }

        }

        private static void handleWebException(WebException e)
        {
            HttpWebResponse errorResp = e.Response as HttpWebResponse;
            Console.ReadKey();
        }
    }
}
