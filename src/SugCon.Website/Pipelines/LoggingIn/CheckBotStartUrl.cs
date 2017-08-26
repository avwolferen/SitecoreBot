namespace SugCon.Website.Pipelines.LoggingIn
{
    using Sitecore;
    using Sitecore.Configuration;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Exceptions;
    using Sitecore.Pipelines.LoggingIn;
    using Sitecore.Security.Accounts;
    using Sitecore.SecurityModel;
    using Sitecore.Web;
    using Sitecore.Xml;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Xml;

    public class CheckBotStartUrl
    {
        private List<string> botHost = Settings.GetSetting("sitecore.bot.host").Split('|').ToList();

        public CheckBotStartUrl()
        {
        }

        /// <summary>
        /// Checks, if the user has read access to the item path.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="itemPath">The item path.</param>
        /// <returns>
        ///   <c>true</c> if the user can acces the item; otherwise, <c>false</c>.
        /// </returns>
        private bool CanAccessItem(Database database, string userName, string itemPath)
        {
            bool flag;
            using (UserSwitcher userSwitcher = new UserSwitcher(User.FromName(userName, false)))
            {
                Item item = database.GetItem(itemPath);
                flag = (item == null ? false : item.Access.CanRead());
            }
            return flag;
        }

        /// <summary>
        /// Checks the on external URL.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="T:Sitecore.Exceptions.SecurityException">Only local URLs are allowed.</exception>
        private void CheckOnExternalUrl(LoggingInArgs args)
        {
            if (WebUtil.IsExternalUrl(args.StartUrl, HttpContext.Current.Request.Url.Host) && !IsAllowedBot(args.StartUrl))
            {
                args.AbortPipeline();
                throw new SecurityException("Only local URLs are allowed.");
            }
        }

        private bool IsAllowedBot(string startUrl)
        {
            Uri startUri = new Uri(startUrl, startUrl.Contains("://") ? UriKind.Absolute : UriKind.Relative);

            return botHost.Contains(startUri.Host.ToLowerInvariant());
        }

        /// <summary>
        /// Gets the alias item.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="localPath">The local path.</param>
        /// <returns>The alias item.</returns>
        private Item GetAliasItem(Database database, string localPath)
        {
            Assert.ArgumentNotNull(database, "database");
            Assert.ArgumentNotNullOrEmpty(localPath, "localPath");
            if (!Settings.AliasesActive)
            {
                return null;
            }
            ID targetID = database.Aliases.GetTargetID(localPath);
            if (targetID.IsNull)
            {
                return null;
            }
            return database.GetItem(targetID);
        }

        /// <summary>
        /// Gets the local path.
        /// </summary>
        private string GetLocalPath(string itemPath)
        {
            Uri uri = new Uri(itemPath, UriKind.Relative);
            uri = new Uri(HttpContext.Current.Request.Url, uri);
            string lowerInvariant = uri.LocalPath.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(lowerInvariant))
            {
                return string.Empty;
            }
            if (lowerInvariant.EndsWith("default.aspx", StringComparison.InvariantCulture))
            {
                lowerInvariant = lowerInvariant.Substring(0, lowerInvariant.Length - "default.aspx".Length);
            }
            else if (lowerInvariant.EndsWith(".aspx", StringComparison.InvariantCulture))
            {
                lowerInvariant = lowerInvariant.Substring(0, lowerInvariant.Length - ".aspx".Length);
            }
            if (lowerInvariant.IndexOf("/sitecore/shell", StringComparison.InvariantCulture) == 0)
            {
                lowerInvariant = lowerInvariant.Replace("/sitecore/shell", "/sitecore/content");
            }
            return lowerInvariant;
        }

        /// <summary>
        /// Checks, if the specified item exist in Core DB.
        /// </summary>
        private bool ItemExist(Database database, string localPath)
        {
            bool flag;
            using (SecurityDisabler securityDisabler = new SecurityDisabler())
            {
                Item item = database.GetItem(localPath) ?? this.GetAliasItem(database, localPath);
                flag = item != null;
            }
            return flag;
        }

        /// <summary>
        /// Runs the processor.
        /// </summary>
        /// <param name="args">The arguments.½</param>
        public void Process(LoggingInArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (!string.IsNullOrEmpty(args.StartUrl))
            {
                this.ValidateStartUrl(args);
            }
        }

        /// <summary>
        /// Determines whether [is valid start URL] [the specified user name].
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>
        ///   <c>true</c> if [is valid start URL] [the specified user name]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool ValidateStartUrl(LoggingInArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            this.CheckOnExternalUrl(args);

            if (string.IsNullOrWhiteSpace(args.StartUrl))
            {
                return true;
            }

            if (Uri.IsWellFormedUriString(args.StartUrl, UriKind.Absolute))
            {
                return true;
            }

            string localPath = this.GetLocalPath(args.StartUrl);
            if (string.IsNullOrWhiteSpace(localPath))
            {
                return true;
            }
            if (ID.IsID(localPath))
            {
                if (this.CanAccessItem(Context.Database, args.Username, localPath))
                {
                    return true;
                }
                args.StartUrl = string.Empty;
                return false;
            }
            if (!this.ItemExist(Context.Database, localPath))
            {
                return true;
            }
            if (this.CanAccessItem(Context.Database, args.Username, localPath))
            {
                return true;
            }
            args.StartUrl = string.Empty;
            return false;
        }
    }
}
