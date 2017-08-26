namespace SugCon.SitecoreBot.Services
{
    using SugCon.Models.Publishing;
    using System;
    using System.Threading.Tasks;

    public class SitecorePublishAPI : BaseAPI
    {
        public SitecorePublishAPI() : base()
        {
        }

        public SitecorePublishAPI(string access_token) : base(access_token)
        {
        }

        public static SitecorePublishAPI Instance(string access_token)
        {
            return new SitecorePublishAPI(access_token);
        }

        public static SitecorePublishAPI Instance()
        {
            return new SitecorePublishAPI();
        }

        public async Task<dynamic> Subscribe()
        {
            var uri = GetUri($"{ SitecoreUrl}/api/publish/subscribe");

            return await GetRequest<dynamic>(uri);
        }

        public async Task<dynamic> UnSubscribe()
        {
            var uri = GetUri($"{ SitecoreUrl}/api/publish/unsubscribe");

            return await GetRequest<dynamic>(uri);
        }

        public async Task<PublishingHandle> SmartPublish()
        {
            var uri = GetUri($"{ SitecoreUrl}/api/publish/smart");

            return await GetRequest<PublishingHandle>(uri);
        }
        
    }
}