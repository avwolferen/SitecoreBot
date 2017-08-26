namespace SugCon.SitecoreBot.Services
{
    using SugCon.Models.Notifications;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class SitecoreIndexAPI : BaseAPI
    {
        public SitecoreIndexAPI() : base()
        {
        }

        public SitecoreIndexAPI(string access_token) : base(access_token)
        {
        }

        public static SitecoreIndexAPI Instance(string access_token)
        {
            return new SitecoreIndexAPI(access_token);
        }

        public static SitecoreIndexAPI Instance()
        {
            return new SitecoreIndexAPI();
        }

        public async Task<IList<IndexingUpdate>> List()
        {
            var uri = GetUri($"{ SitecoreUrl}/api/index/list");

            return await GetRequest<IList<IndexingUpdate>>(uri);
        }

        public async Task<IndexingUpdate> Rebuild(string indexname)
        {
            var uri = GetUri($"{ SitecoreUrl}/api/index/rebuild");

            return await PostRequest<string, IndexingUpdate>(uri, indexname);
        }

    }
}