namespace SugCon.SitecoreBot.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using SugCon.Models.Authorization;
    using SugCon.SitecoreBot.ExtensionMethods;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class RootDialog
    {
        [LuisIntent(Constants.Intents.AppInsights_Traces)]
        public async Task AppInsightsTraces(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (context.IsAdministrator() || context.IsInRole(RoleNames.BotAnalytics))
            {
                int top = 25;
                EntityRecommendation entity;
                if (result.TryFindEntity(Constants.Entities.Top, out entity))
                {
                    int.TryParse(entity.Entity, out top);
                }

                await context.PostAsync($"Hold on! I'm pulling the last {top} traces from the cloud!");
                await context.PostAsync(context.CreateTypingActivity());

                var profile = context.GetProfile();
                var service = new Connector.AppInsights.ApplicationInsightsService(profile.ApplicationInsights.ApplicationId, profile.ApplicationInsights.ApiKey);

                var traces = service.GetTraces(new TimeSpan(1, 0, 0), top).ToList();

                StringBuilder sb = new StringBuilder();

                traces.ForEach(trace =>
                {
                    sb.AppendLine($"{trace.timestamp} {trace.trace.message}  \n");
                });

                await context.PostAsync(sb.ToString());
            }
        }
    }
}