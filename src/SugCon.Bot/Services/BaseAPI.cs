namespace SugCon.SitecoreBot.Services
{
    using Newtonsoft.Json;
    using SugCon.SitecoreBot.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;


    public class BaseAPI
    {
        public readonly string SitecoreAppId = ConfigurationManager.AppSettings["sitecore.app.id"];
        public readonly string SitecoreAppSecret = ConfigurationManager.AppSettings["sitecore.app.secret"];
        public readonly string SitecoreUrl = ConfigurationManager.AppSettings["sitecore.url"];
        private readonly string access_token;

        public BaseAPI()
        {
        }

        public BaseAPI(string access_token)
        {
            this.access_token = access_token;
        }

        public static string TokenEncoder(string token)
        {
            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(StringCipher.Encrypt(token)));
        }

        public static string TokenDecoder(string token)
        {
            return StringCipher.Decrypt(Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(token)));
        }

        public async Task<T> GetRequest<T>(Uri uri, string override_access_token = null)
        {
            string json;
            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(override_access_token) || !string.IsNullOrWhiteSpace(access_token))
                {
                    client.DefaultRequestHeaders.Add("X-AccessToken", override_access_token ?? access_token);
                }

                json = await client.GetStringAsync(uri).ConfigureAwait(false);
            }

            try
            {
                var result = JsonConvert.DeserializeObject<T>(json);
                return result;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Unable to deserialize the Sitecore response.", ex);
            }
        }

        public async Task<TResponse> PostRequest<TRequest, TResponse>(Uri uri, TRequest body)
        {
            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(access_token))
                {
                    client.DefaultRequestHeaders.Add("X-AccessToken", access_token);
                }

                var response = await client.PostAsJsonAsync(uri, body).ConfigureAwait(false);
                return await response.Content.ReadAsAsync<TResponse>();
            }
        }

        public async Task<TResponse> DeleteRequest<TResponse>(Uri uri)
        {
            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(access_token))
                {
                    client.DefaultRequestHeaders.Add("X-AccessToken", access_token);
                }

                var response = await client.DeleteAsync(uri).ConfigureAwait(false);
                return await response.Content.ReadAsAsync<TResponse>();
            }
        }

        public static Uri GetUri(string endPoint, params Tuple<string, string>[] queryParams)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach (var queryparam in queryParams)
            {
                queryString[queryparam.Item1] = queryparam.Item2;
            }

            var builder = new UriBuilder(endPoint);
            builder.Query = queryString.ToString();
            return builder.Uri;
        }
    }
}