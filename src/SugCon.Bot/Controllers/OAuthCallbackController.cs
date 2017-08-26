namespace SugCon.SitecoreBot.Controllers
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Bot.Connector;
    using System.Web;
    using SugCon.SitecoreBot.Services;

    public class OAuthCallbackController : ApiController
    {
        public static Uri OauthCallbackUri
        {
            get
            {
                Uri currentUri = HttpContext.Current.Request.Url;
                return new Uri(string.Concat(currentUri.Scheme, "://", currentUri.Authority, "/api/OAuthCallback"));
            }
        }

        /// <summary>
        /// OAuth call back that is called by Sitecore.
        /// </summary>
        /// <param name="userId"> The Id for the user that is getting authenticated.</param>
        /// <param name="botId">The Id for the bot</param>
        /// <param name="conversationId"> The Id of the conversation.</param>
        /// <param name="code"> The Authentication code returned by Facebook.</param>
        /// <param name="state"> The state returned by Facebook.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/OAuthCallback")]
        public async Task<HttpResponseMessage> OAuthCallback([FromUri] string userId, [FromUri] string botId, [FromUri] string conversationId, [FromUri] string channelId, [FromUri] string serviceUrl, [FromUri] string code, CancellationToken token)
        {
            var conversationReference = new ConversationReference
                (
                    user: new ChannelAccount(id: SitecoreAuthenticationAPI.TokenDecoder(userId)),
                    bot: new ChannelAccount(id: SitecoreAuthenticationAPI.TokenDecoder(botId)),
                    conversation: new ConversationAccount(id: SitecoreAuthenticationAPI.TokenDecoder(conversationId)),
                    channelId: SitecoreAuthenticationAPI.TokenDecoder(channelId),
                    serviceUrl: SitecoreAuthenticationAPI.TokenDecoder(serviceUrl)
                );

            // Exchange the Sitecore Auth code with Access token
            var accessToken = await SitecoreAuthenticationAPI.Instance().ExchangeCodeForAccessToken(conversationReference, code);

            // Create the message that is send to conversation to resume the login flow
            var msg = conversationReference.GetPostToUserMessage();

            msg.Text = $"token:{accessToken.AccessToken}";

            //// Resume the conversation to AuthenticationDialog
            await Conversation.ResumeAsync(conversationReference, msg);

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, msg))
            {
                return Request.CreateResponse("You are now logged in! Continue talking to the bot.");
            }
        }
    }
}