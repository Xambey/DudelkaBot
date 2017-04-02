﻿using Microsoft.AspNetCore.Http;
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

namespace DudelkaBot.ircClient
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
        private static string DjURL = "https://twitch-dj.ru/includes/back.php?func=playlist&channel=channel_id&c";
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

        private string UniqueMessageId()
        {
            string result = "";
            for (int i = 0; i < lengthid; i++)
            {
                result += patternrandom[random.Next(0, 62)];
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel_id"> channel id from twitch-dj.ru/channelname , see back.php GET request url </param>
        public async Task<string> GetMusicFromTwitchDJ(string channel_id)
        {
            HttpResponseMessage message;
            try
            {
                HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, DjURL.Replace("channel_id", channel_id));
                m.Method = new HttpMethod("GET");
                m.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                m.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };
                message = await client.SendAsync(m);
            }
            catch (WebException ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                return "Ошибка, говнокод! Обратитесь к моему хозяину!";
            }
            string te = Encoding.UTF8.GetString(message.Content.ReadAsByteArrayAsync().Result);
            if (te == "null" || te == null)
                return string.Empty;
            JObject text = JObject.Parse(te);
            var results = text["1"].Children().ToList();
            string status = results[5].ToObject<string>();
            if (status != "1")
                return "Сейчас музыка не играет FeelsBadMan ";
            string title = results[2].ToObject<string>();
            string author = results[6].ToObject<string>();
            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(author))
                return $"Сейчас играет: {title} Kreygasm Заказал: {author} Kappa ";
            return string.Empty;
        }


        public async void GetChannelStatus(string to_id, string message)
        {
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
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                return;
            }
        }

        public async void SendPostWhisperMessage(string to_id, string message)
        {
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
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                return;
            }
        }

        internal async Task<string> MakeGetRequest()
        {
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
            catch (WebException e) {
                handleWebException(e);
                return null;
            }

        }

        internal async Task<string> MakePostRequest(string message ,ulong to_userid , string method, string requestData = null, byte[] data = null)
        {                

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
            catch (WebException e) {
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
