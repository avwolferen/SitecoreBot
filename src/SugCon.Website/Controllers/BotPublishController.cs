namespace SugCon.Website.Controllers
{
    using MongoDB.Driver.Builders;
    using Newtonsoft.Json;
    using Sitecore;
    using Sitecore.Abstractions;
    using Sitecore.Analytics.Model;
    using Sitecore.Data;
    using Sitecore.Data.Managers;
    using Sitecore.Globalization;
    using Sitecore.Security.Accounts;
    using SugCon.Models.Authorization;
    using System;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Results;

    public class BotPublishController : ApiControllerBase
    {
        [HttpGet]
        [Route("_bot/api/publish/subscribe")]
        public IHttpActionResult Subscribe()
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

                return new JsonResult<bool>(true, new JsonSerializerSettings(), Encoding.UTF8, this);
            }
        }

        [HttpGet]
        [Route("_bot/api/publish/unsubscribe")]
        public IHttpActionResult UnSubscribe()
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

                return new JsonResult<bool>(true, new JsonSerializerSettings(), Encoding.UTF8, this);
            }
        }

        [HttpGet]
        [Route("_bot/api/publish/smart")]
        public IHttpActionResult SmartPublish()
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

                Database[] targets = new Database[] { Sitecore.Configuration.Factory.GetDatabase("web") };
                Database master = Sitecore.Configuration.Factory.GetDatabase("master");
                Language[] languages = LanguageManager.GetLanguages(master).ToArray();

                Handle publishHandle = Sitecore.Publishing.PublishManager.PublishSmart(master, targets, languages);

                return new JsonResult<Handle>(publishHandle, new JsonSerializerSettings(), Encoding.UTF8, this);
            }
        }
    }
}