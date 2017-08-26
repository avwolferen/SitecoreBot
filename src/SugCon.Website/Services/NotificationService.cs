namespace SugCon.Website.Services
{
    using SugCon.Models.Notifications;
    using SugCon.Models.Publishing;
    using System;
    using System.Net.Http;

    public class NotificationService<T> where T : class
    {
        private string _callbackBaseUrl = Sitecore.Configuration.Settings.GetSetting("sitecore.bot.callbackhost");

        /// <summary>
        /// POST the payload to the notification endpoint of the BOT
        /// </summary>
        /// <param name="payload"></param>
        public void Send(T payload)
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = new Uri(string.Concat(_callbackBaseUrl, GetNotifyAction(typeof(T))));
                var response = client.PostAsJsonAsync(uri, payload).Result;
            }
        }

        private string GetNotifyAction(Type t)
        {
            if (t == typeof(PublishUpdate))
            {
                return "/api/notify/publish";
            }
            else if (t == typeof(IndexingUpdate))
            {
                return "/api/notify/index";
            }

            return string.Empty;
        }
    }
}