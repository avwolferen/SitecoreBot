namespace SugCon.SitecoreBot.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using SugCon.Models.Authorization;
    using SugCon.SitecoreBot.ExtensionMethods;
    using SugCon.SitecoreBot.Helpers;
    using SugCon.SitecoreBot.Services;
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class RootDialog
    {
        [LuisIntent(Constants.Intents.XDB_Sessions)]
        public async Task XDBSessions(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (context.IsAdministrator() || context.IsInRole(RoleNames.BotAnalytics))
            {
                EntityRecommendation period;
                Chronic.Span span = null;

                if (result.TryFindEntity(Constants.Entities.BuiltIn_DateTime, out period))
                {
                    var parser = new Chronic.Parser();
                    foreach (var resolution in period.Resolution)
                    {
                        if (new Regex(@"^(201[0-9])$").IsMatch(resolution.Value))
                        {
                            try
                            {
                                var year = int.Parse(resolution.Value.Substring(0, 4));
                                span = new Chronic.Span(new DateTime(year, 1, 1), new DateTime(year, 1, 1).AddYears(1));
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            span = parser.Parse(resolution.Value);
                        }
                    }

                    if (span == null && period.Resolution.Count > 0)
                    {
                        var resolution = period.Resolution.FirstOrDefault();
                        if (new Regex(@"^(201[0-9]\-W([0-5][0-9]))$").IsMatch(resolution.Value))
                        {
                            try
                            {
                                var year = int.Parse(resolution.Value.Substring(0, 4));
                                var week = int.Parse(resolution.Value.Substring(6));
                                span = new Chronic.Span(DateHelpers.FirstDateOfWeek(year, week), DateHelpers.LastDateOfWeek(year, week));
                            }
                            catch
                            {
                            }
                        }
                        else if (new Regex(@"^(201[0-9]\-([0-1][0-9]))$").IsMatch(resolution.Value))
                        {
                            try
                            {
                                var year = int.Parse(resolution.Value.Substring(0, 4));
                                var month = int.Parse(resolution.Value.Substring(5));
                                span = new Chronic.Span(new DateTime(year, month, 1) , new DateTime(year, month,1).AddMonths(1));
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                if (span == null)
                {
                    span = new Chronic.Span(DateTime.Now, DateTime.Now.AddDays(1));
                }

                var response = await SitecoreXdbAPI.Instance(context.AccessToken()).Interactions(span.Start, span.End);

                if (response.Count > 0)
                {
                    var contacts = response.Select(i => i.ContactId).Distinct();

                    await context.PostAsync($"Between {span.Start.Value.ToString("dd-MM-yyyy")} and {span.End.Value.ToString("dd-MM-yyyy")} we've had a total of {contacts.Count()} unique contacts in {response.Count} interactions. Let's look at some facts.");

                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine($"The average value was {Math.Round((double)response.Sum(i => i.Value) / response.Count, 1, MidpointRounding.ToEven)}  \n");
                    sb.AppendLine($"The average number of pages visited was {Math.Round((double)response.Sum(i => i.VisitPageCount) / response.Count, 1, MidpointRounding.ToEven)}  \n");

                    if (response.Any(i => i.CampaignId.HasValue))
                    {
                        sb.AppendLine($"{response.Count(i => i.CampaignId.HasValue)} triggered a campaign.  \n");
                    }
                    else
                    {
                        sb.AppendLine($"None of them triggered a campaign.  \n");
                    }

                    await context.PostAsync(sb.ToString());
                }
                else
                {
                    await context.PostAsync($"I'm very sorry to say that you haven't had any interactions between {span.Start} and {span.End}");
                }
            }
        }
    }
}