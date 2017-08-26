namespace SugCon.SitecoreBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class SitecoreXdbAPI : BaseAPI
    {
        public SitecoreXdbAPI() : base()
        {
        }

        public SitecoreXdbAPI(string access_token) : base(access_token)
        {
        }

        public static SitecoreXdbAPI Instance(string access_token)
        {
            return new SitecoreXdbAPI(access_token);
        }

        public static SitecoreXdbAPI Instance()
        {
            return new SitecoreXdbAPI();
        }

        public async Task<List<SugCon.Models.Analytics.Interaction>> Interactions(DateTime? startdate = null, DateTime? enddate = null, string site = "website")
        {
            var uri = GetUri($"{ SitecoreUrl}/api/xdb/interactions",
                Tuple.Create("startdate", startdate.HasValue ? startdate.Value.ToString("yyyy-MM-dd") : string.Empty),
                Tuple.Create("enddate", enddate.HasValue ? enddate.Value.ToString("yyyy-MM-dd") : string.Empty),
                Tuple.Create("site", site)
                );

            return await GetRequest<List<SugCon.Models.Analytics.Interaction>>(uri);
        }
    }
}