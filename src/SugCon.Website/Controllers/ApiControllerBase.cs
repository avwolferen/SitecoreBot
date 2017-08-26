using SugCon.Website.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace SugCon.Website.Controllers
{
    public class ApiControllerBase : ApiController
    {
        private readonly string sitecoreBotAppId = Sitecore.Configuration.Settings.GetSetting("sitecore.bot.appId");
        private readonly string sitecoreBotAppSecret = Sitecore.Configuration.Settings.GetSetting("sitecore.bot.appSecret");

        public readonly ITokenService tokenService;

        public ApiControllerBase() : this(new TokenService())
        {
        }

        public ApiControllerBase(ITokenService tokenService)
        {
            this.tokenService = tokenService;
        }

        public string AccessToken
        {
            get
            {
                return HttpContext.Current.Request.Headers["x-accesstoken"];
            }
        }

        public string ApiUser
        {
            get
            {
                return tokenService.TokenDecoder(AccessToken);
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ApiUser);
            }
        }

        public bool IsValidBotSecret(string appId, string secret)
        {
            return !string.IsNullOrWhiteSpace(appId) && appId == sitecoreBotAppId && !string.IsNullOrWhiteSpace(secret) && secret == sitecoreBotAppSecret;
        }
    }
}