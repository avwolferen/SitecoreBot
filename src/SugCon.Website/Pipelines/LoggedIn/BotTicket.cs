using Sitecore.Diagnostics;
using Sitecore.Pipelines.LoggedIn;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace SugCon.Website.Pipelines.LoggedIn
{
    public class BotTicket : LoggedInProcessor
    {
        private List<string> botHost = Sitecore.Configuration.Settings.GetSetting("sitecore.bot.host").Split('|').ToList();

        public BotTicket()
        {

        }

        private bool IsAllowedBot(string startUrl)
        {
            Uri startUri = new Uri(startUrl, startUrl.Contains("://") ? UriKind.Absolute : UriKind.Relative);
            return botHost.Contains(startUri.Host.ToLowerInvariant());
        }

        public override void Process(LoggedInArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            if (!Uri.IsWellFormedUriString(args.StartUrl, UriKind.Relative) && IsAllowedBot(args.StartUrl))
            {
                args.AbortPipeline();
            }
        }

    }
}