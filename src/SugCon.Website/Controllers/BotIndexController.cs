namespace SugCon.Website.Controllers
{
    using Newtonsoft.Json;
    using Sitecore;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Maintenance;
    using Sitecore.ContentSearch.Utilities;
    using Sitecore.Security.Accounts;
    using SugCon.Models.Notifications;
    using SugCon.Website.Events;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Results;

    public class BotIndexController : ApiControllerBase
    {
        [HttpGet]
        [Route("_bot/api/index/list")]
        public IHttpActionResult List()
        {
            if (!IsAuthenticated)
            {
                return Unauthorized();
            }

            using (new UserSwitcher(Context.Site.Domain.GetFullName(this.ApiUser), false))
            {
                if (!Context.User.IsAdministrator)
                {
                    return Unauthorized();
                }

                IList<IndexingUpdate> indexes = ContentSearchManager.Indexes.Where(index => 
                {
                    return index.Group != IndexGroup.Experience && ContentSearchManager.Locator.GetInstance<ISearchIndexSwitchTracker>().IsIndexOn(index.Name);
                })
                .Select(index =>
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine(IndexingEventHandler.BuildTime(index));
                    sb.AppendLine(IndexingEventHandler.ThroughPut(index));
                    sb.AppendLine(string.Concat("Document count: ", index.Summary.NumberOfDocuments));
                    sb.AppendLine(string.Format("Last Updated: {0} (UTC)", string.Concat(index.Summary.LastUpdated.ToShortDateString(), " - ", index.Summary.LastUpdated.ToShortTimeString())));

                    var payload = new IndexingUpdate
                    {
                        IndexName = index.Name,
                        Message = sb.ToString(),
                        State = State.UnKnown,
                        NumberOfDocuments = index.Summary.NumberOfDocuments,
                        LastUpdated = index.Summary.LastUpdated,
                        IndexRebuildMilliseconds = IndexHealthHelper.GetIndexRebuildTime(index.Name),
                        IndexRebuildTime = IndexingEventHandler.BuildTime(index)
                    };

                    return payload;
                }).ToList();

                return new JsonResult<IList<IndexingUpdate>>(indexes, new JsonSerializerSettings(), Encoding.UTF8, this);
            }
        }

        [HttpPost]
        [Route("_bot/api/index/rebuild")]
        public IHttpActionResult Rebuild([FromBody] string indexname)
        {
            if (!IsAuthenticated)
            {
                return Unauthorized();
            }

            using (new UserSwitcher(Context.Site.Domain.GetFullName(this.ApiUser), false))
            {
                if (!Context.User.IsAdministrator)
                {
                    return Unauthorized();
                }

                var index = ContentSearchManager.Indexes.SingleOrDefault(idx => idx.Name == indexname);

                var payload = new IndexingUpdate
                {
                    IndexName = index.Name,
                    State = State.Started,
                    NumberOfDocuments = index.Summary.NumberOfDocuments,
                    LastUpdated = index.Summary.LastUpdated,
                    IndexRebuildMilliseconds = IndexHealthHelper.GetIndexRebuildTime(index.Name),
                    IndexRebuildTime = IndexingEventHandler.BuildTime(index)
                };

                if (index.Group == IndexGroup.Experience || !ContentSearchManager.Locator.GetInstance<ISearchIndexSwitchTracker>().IsIndexOn(index.Name))
                {
                    payload.State = State.NotStarting;
                    return new JsonResult<IndexingUpdate>(payload, new JsonSerializerSettings(), Encoding.UTF8, this);
                }
                else
                {
                    var job = IndexCustodian.FullRebuild(index, true);
                    payload.Job = job.DisplayName;
                    payload.State = State.Started;
                    return new JsonResult<IndexingUpdate>(payload, new JsonSerializerSettings(), Encoding.UTF8, this);
                }
            }
        }
    }
}