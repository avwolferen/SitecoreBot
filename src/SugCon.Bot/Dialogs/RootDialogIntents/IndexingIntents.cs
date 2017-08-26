namespace SugCon.SitecoreBot.Dialogs
{
    using AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Microsoft.Bot.Connector;
    using Newtonsoft.Json;
    using SugCon.SitecoreBot.Constants;
    using SugCon.SitecoreBot.Controllers;
    using SugCon.SitecoreBot.ExtensionMethods;
    using SugCon.SitecoreBot.Services;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class RootDialog
    {
        [LuisIntent(Intents.List_Indexes)]
        public async Task ListIndexes(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (context.IsAdministrator())
            {
                var response = await SitecoreIndexAPI.Instance(context.AccessToken()).List();

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("I've got the following indexes for you  \n");

                response.ToList().ForEach(index =>
                {
                    sb.AppendLine($"**{index.IndexName}**  \n");
                    sb.AppendLine($"*Documents: {index.NumberOfDocuments}*  \n");
                    sb.AppendLine($"*Rebuild time: {index.IndexRebuildTime}*  \n");
                    sb.AppendLine($"  \n");
                    sb.AppendLine($"  \n");
                });

                var reply = context.MakeMessage();

                reply.Text = sb.ToString();

                await context.PostAsync(reply);
            }
        }

        [LuisIntent(Intents.Rebuild_Index)]
        public async Task RebuildIndex(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (context.IsAdministrator())
            {
                EntityRecommendation index;
                result.TryFindEntity(Entities.Index_Name, out index);

                if (index == null || string.IsNullOrWhiteSpace(index.Entity))
                {
                    await context.PostAsync($"I'm sorry, I wasn't able to recognize the index you wanted to rebuild.");
                }
                else
                {
                    var response = await SitecoreIndexAPI.Instance(context.AccessToken()).Rebuild(index.Entity.Replace(" ", string.Empty));

                    if (!NotificationController.SubscribedIndexingConversations.ContainsKey(context.Activity.Conversation.Id))
                    {
                        if (response.IndexRebuildMilliseconds == 0)
                        {
                            await context.PostAsync($"Rebuilding for {response.IndexName} started. I don't know when it's ready, it hasn't been indexed before");
                        }
                        else
                        {
                            await context.PostAsync($"Rebuilding for {response.IndexName} started. Should be ready in approximately {response.IndexRebuildTime} :)");
                        }

                        await context.PostAsync("If you would like updates on index rebuilds just ask me!");
                    }
                }
            }
        }

        [LuisIntent(Intents.SubscribeToIndexing)]
        public async Task SubscribeToIndexing(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (context.IsAdministrator())
            {
                NotificationController.SubscribeToIndexingUpdates(context);

                await context.PostAsync($"Subscribed to indexing events!");
            }
        }

        [LuisIntent(Intents.UnSubscribeFromIndexing)]
        public async Task UnSubscribeFromIndexing(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (context.IsAdministrator())
            {
                NotificationController.UnSubscribeFromIndexingUpdates(context);

                await context.PostAsync($"Unsubscribed from indexing events");
            }
        }
    }
}