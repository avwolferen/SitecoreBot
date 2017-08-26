using Newtonsoft.Json;
using SugCon.Website.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Http.Results;

namespace SugCon.Website.Controllers
{
    public class BotAuthenticationController : ApiControllerBase
    {
        private readonly List<string> sitecoreBotHost = Sitecore.Configuration.Settings.GetSetting("sitecore.bot.host").Split('|').ToList();

        public BotAuthenticationController() : base(new TokenService())
        {
        }

        public BotAuthenticationController(ITokenService tokenService) : base(tokenService)
        {
        }

        [Route("_bot/api/authentication/ticket")]
        [HttpPost]
        public IHttpActionResult GetTicket([FromBody] string callback)
        {
            if (string.IsNullOrWhiteSpace(callback))
            {
                return Unauthorized();
            }

            string ticket = GenerateTicket(64, new Random());
            AddTicket(ticket, callback);
            return new JsonResult<string>(ticket, new JsonSerializerSettings(), Encoding.UTF8, this);
        }

        public static void AddTicket(string ticket, string callback)
        {
            HttpRuntime.Cache.Add(ticket, callback, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public static string GetTicketCallback(string ticket)
        {
            return HttpRuntime.Cache.Get(ticket) as string;
        }

        [Route("_bot/api/authentication/access_token")]
        [HttpGet]
        public IHttpActionResult GetAccessToken([FromUri] string client_id, [FromUri] string redirect_uri, [FromUri] string client_secret, [FromUri] string code)
        {
            if (string.IsNullOrWhiteSpace(client_id) || string.IsNullOrWhiteSpace(client_secret) || string.IsNullOrWhiteSpace(redirect_uri) || string.IsNullOrWhiteSpace(code))
            {
                return Unauthorized();
            }

            if (!IsValid(client_id, client_secret, redirect_uri))
            {
                return Unauthorized();
            }

            string username = tokenService.TokenDecoder(code);
            
            TimeSpan expire = new TimeSpan(1,0,0);
            var token = new Models.SitecoreAccessToken
            {
                AccessToken = tokenService.TokenEncoder(username, expire),
                ExpiresIn = expire.Ticks,
                TokenType = "bot"
            };

            return new JsonResult<Models.SitecoreAccessToken>(token, new JsonSerializerSettings(), Encoding.UTF8, this);
        }

        [HttpGet]
        [Route("_bot/api/authentication/validate_token")]
        public IHttpActionResult ValidateToken([FromUri] string access_token)
        {
            var input_token = HttpContext.Current.Request.Headers["x-accesstoken"];
            string token = tokenService.TokenDecoder(input_token);

            if (string.IsNullOrWhiteSpace(token))
            {
                return new JsonResult<bool>(false, new JsonSerializerSettings(), Encoding.UTF8, this);
            }

            if (!IsValidBotSecret(access_token.Split('|').First(), access_token.Split('|').Last()))
            {
                return new JsonResult<bool>(false, new JsonSerializerSettings(), Encoding.UTF8, this);
            }

            return new JsonResult<bool>(true, new JsonSerializerSettings(), Encoding.UTF8, this);
        }

        private bool IsValid(string client_id, string client_secret, string redirect_uri)
        {
            if (!IsValidBotSecret(client_id, client_secret))
            {
                return false;
            }

            return Uri.IsWellFormedUriString(redirect_uri, UriKind.Absolute) 
                && sitecoreBotHost.Contains(new Uri(redirect_uri).Host.ToLowerInvariant());
        }

        public static string GenerateTicket(int length, Random random)
        {
            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(characters[random.Next(characters.Length)]);
            }

            return result.ToString();
        }
    }
}