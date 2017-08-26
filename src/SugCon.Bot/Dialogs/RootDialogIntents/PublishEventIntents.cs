namespace SugCon.SitecoreBot.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using SugCon.SitecoreBot.Controllers;
    using SugCon.SitecoreBot.ExtensionMethods;
    using SugCon.SitecoreBot.Services;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class RootDialog
    {
        [LuisIntent(Constants.Intents.SubscribeToPublish)]
        public async Task SubscribeToPublishEvent(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (context.IsAdministrator())
            {
                var response = await SitecorePublishAPI.Instance(context.AccessToken()).Subscribe();

                NotificationController.SubscribeToPublishUpdates(context);

                await context.PostAsync($"Subscribed to publish events!");
            }
        }

        [LuisIntent(Constants.Intents.UnSubscribeFromPublish)]
        public async Task UnSubscribeFromPublishEvent(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (context.IsAdministrator())
            {
                var response = await SitecorePublishAPI.Instance(context.AccessToken()).UnSubscribe();

                NotificationController.UnSubscribeToPublishUpdates(context);
               
                await context.PostAsync($"Unsubscribed from publish events");
            }
        }

        [LuisIntent(Constants.Intents.SmartPublish)]
        public async Task SmartPublish(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (context.IsAdministrator())
            {
                var response = await SitecorePublishAPI.Instance(context.AccessToken()).SmartPublish();

                if (!NotificationController.SubscribedPublishConversations.ContainsKey(context.Activity.Conversation.Id))
                {
                    await context.PostAsync($"Alright, you have just started a smart publish on {response.ProducedByInstanceName}");
                    await context.PostAsync("If you would like to receive a notification for publish event just ask me!");
                }
            }
        }
    }
}