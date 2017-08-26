namespace SugCon.SitecoreBot.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Reflection;
    using Microsoft.Bot.Connector;
    using SugCon.SitecoreBot.ExtensionMethods;

    using SugCon.SitecoreBot.Helpers;
    using SugCon.SitecoreBot.Services;

    [Serializable]
    public class AuthenticationDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceived);
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            var userprofile = context.GetProfile();
            if (userprofile == null || string.IsNullOrWhiteSpace(userprofile.AccessToken))
            {
                if (message.Text.StartsWith("token:"))
                {
                    var token = message.Text.Remove(0, "token:".Length);
                    var validtoken = await SitecoreAuthenticationAPI.Instance().ValidateAccessToken(token);
                    if (validtoken)
                    {
                        var profile = await SitecoreUserManagementAPI.Instance(token).GetProfile();
                        userprofile = new Models.UserProfile
                        {
                            AccessToken = token,
                            FullName = profile.FullName,
                            Login = profile.UserName,
                            IsAdministrator =profile.IsAdministrator,
                            Roles = profile.Roles,
                            ApplicationInsights = profile.ApplicationInsights
                        };

                        context.SetProfile(userprofile);
                        context.Done(context.GetUnauthorizedMessageText());
                        return;
                    }
                }

                await context.PostAsync("Hi I'm Sitecore Bot! I don't really know who you are, so you have to show me some proof!");
                await LogIn(context);
                context.Wait(MessageReceived);
                return;
            }

            var authenticated = await userprofile.IsAuthenticated();
            if (!authenticated)
            {
                userprofile.AccessToken = null;
                context.SetProfile(userprofile);

                await context.PostAsync("Okay, I don't really remember who you were, could you please identify yourself?");
                await LogIn(context);
                context.Wait(MessageReceived);
                return;
            }

            return;
        }

        private async Task LogIn(IDialogContext context)
        {
            await context.PostAsync(context.CreateTypingActivity());

            var ticket = await SitecoreAuthenticationAPI.Instance().GetTicket(new ConversationReference(
                    user: new ChannelAccount(id: context.Activity.From.Id),
                    conversation: new ConversationAccount(id: context.Activity.Conversation.Id),
                    bot: new ChannelAccount(id: context.Activity.Recipient.Id),
                    channelId: context.Activity.ChannelId,
                    serviceUrl: context.Activity.ServiceUrl));

            var sitecoreLoginUrl = SitecoreAuthenticationAPI.Instance().GetSitecoreLoginURL(ticket);

            var reply = context.MakeMessage();

            reply.Attachments = new List<Attachment>();

            List<CardAction> cardButtons = new List<CardAction>();
            CardAction loginButton = new CardAction()
            {
                Value = sitecoreLoginUrl,
                Type = "openUrl",
                Title = "login"
            };

            cardButtons.Add(loginButton);

            SigninCard plCard = new SigninCard(text: "Trust me, I'm not standing here with Bobby Hack!!", buttons: cardButtons);

            reply.Attachments.Add(plCard.ToAttachment());

            await context.PostAsync(reply);
        }
    }
}