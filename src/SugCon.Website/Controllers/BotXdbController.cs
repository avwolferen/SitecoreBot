namespace SugCon.Website.Controllers
{
    using MongoDB.Driver.Builders;
    using Newtonsoft.Json;
    using Sitecore;
    using Sitecore.Analytics.Model;
    using Sitecore.Security.Accounts;
    using SugCon.Models.Analytics;
    using SugCon.Models.Authorization;
    using System;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Results;

    public class BotXdbController : ApiControllerBase
    {
        [HttpGet]
        [Route("_bot/api/xdb/interactions")]
        public IHttpActionResult GetInteractions([FromUri] DateTime? startdate, [FromUri] DateTime? enddate, [FromUri] string site = "website")
        {
            if (!IsAuthenticated)
            {
                return Unauthorized();
            }

            using (new UserSwitcher(Context.Site.Domain.GetFullName(this.ApiUser), false))
            {
                if (!Context.User.IsAdministrator && !Context.User.IsInRole((Role.FromName(RoleNames.BotAnalytics))))
                {
                    return Unauthorized();
                }
                                
                if (startdate.HasValue && enddate.HasValue && enddate.Value < startdate.Value)
                {
                    enddate = DateTime.MaxValue;
                }
                else if (!startdate.HasValue)
                {
                    return new JsonResult<object>(null, new JsonSerializerSettings(), Encoding.UTF8, this);
                }

                //Connecting to the Analytics DB
                var driver = Sitecore.Analytics.Data.DataAccess.MongoDb.MongoDbDriver.FromConnectionString("analytics");

                //Building our query
                var builder = new QueryBuilder<VisitData>();

                var filter = builder.And(
                        builder.GTE(interaction => interaction.StartDateTime, startdate)
                        , builder.LTE(interaction => interaction.EndDateTime, enddate)
                        , builder.EQ(interaction => interaction.SiteName, site.ToLower())
                    );

                //Retrieving data from the "Interactions" collection
                var interactions = driver.Interactions.FindAs<Interaction>(filter);
               
                return new JsonResult<object>(interactions, new JsonSerializerSettings(), Encoding.UTF8, this);
            }
        }
    }
}