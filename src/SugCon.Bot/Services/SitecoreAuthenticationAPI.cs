namespace SugCon.SitecoreBot.Services
{
    using Microsoft.Bot.Builder.Dialogs;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Configuration;
    using Models;
    using Microsoft.Bot.Connector;
    using SugCon.Models;
    using SugCon.Models.UserManagement;

    public class SitecoreAuthenticationAPI : BaseAPI
    {
        public SitecoreAuthenticationAPI() : base()
        {
        }

        public SitecoreAuthenticationAPI(string access_token) : base(access_token)
        {
        }

        public static SitecoreAuthenticationAPI Instance(string access_token)
        {
            return new SitecoreAuthenticationAPI(access_token);
        }

        public static SitecoreAuthenticationAPI Instance()
        {
            return new SitecoreAuthenticationAPI();
        }

        public async Task<SitecoreAccessToken> ExchangeCodeForAccessToken(ConversationReference conversationReference, string code)
        {
            var uri = GetUri($"{SitecoreUrl}/api/authentication/access_token",
                Tuple.Create("client_id", SitecoreAppId),
                Tuple.Create("redirect_uri", Controllers.OAuthCallbackController.OauthCallbackUri.ToString()),
                Tuple.Create("client_secret", SitecoreAppSecret),
                Tuple.Create("code", code));

            return await GetRequest<SitecoreAccessToken>(uri, null);
        }

        public async Task<bool> ValidateAccessToken(string token)
        {
            var uri = GetUri($"{SitecoreUrl}/api/authentication/validate_token",
                Tuple.Create("access_token", $"{SitecoreAppId}|{SitecoreAppSecret}"));

            var res = await GetRequest<object>(uri, token).ConfigureAwait(false);
            return (bool)res;
        }

        public string GetSitecoreLoginURL(string ticket)
        {
            var uri = GetUri($"{SitecoreUrl}",
                Tuple.Create("client_id", SitecoreAppId),
                Tuple.Create("ticket", ticket),
                Tuple.Create("state", Convert.ToString(new Random().Next(9999)))
                );

            return uri.ToString();
        }

        public async Task<string> GetTicket(ConversationReference conversationReference)
        {
            
            var callback = GetUri(Controllers.OAuthCallbackController.OauthCallbackUri.ToString(),
               Tuple.Create("userId", TokenEncoder(conversationReference.User.Id)),
               Tuple.Create("botId", TokenEncoder(conversationReference.Bot.Id)),
               Tuple.Create("conversationId", TokenEncoder(conversationReference.Conversation.Id)),
               Tuple.Create("serviceUrl", TokenEncoder(conversationReference.ServiceUrl)),
               Tuple.Create("channelId", TokenEncoder(conversationReference.ChannelId))
               ).ToString();

            var uri = GetUri($"{SitecoreUrl}/api/authentication/ticket");

            var ticket = await PostRequest<string, string>(uri, callback);
            return ticket;
        }
    }
}