namespace SugCon.Website.Bot
{
    using Services;
    using Sitecore;
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Pipelines;
    using Sitecore.Pipelines.LoggedIn;
    using Sitecore.Pipelines.LoggingIn;
    using Sitecore.Security.Accounts;
    using Sitecore.SecurityModel.Cryptography;
    using Sitecore.Web;
    using System;
    using System.Net;
    using System.Text.RegularExpressions;

    public partial class Default : System.Web.UI.Page
    {
        private string fullUserName = string.Empty;

        private string startUrl = string.Empty;

        private readonly ITokenService tokenService;

        public Default()
        {
            tokenService = new TokenService();
        }

        /// <summary>
        /// Returns the path to the background image for use on the login page
        /// </summary>
        /// <returns>Image url</returns>
        protected string GetBackgroundImageUrl()
        {
            return "/_bot/Drop_Wallpaper_bot.jpg"; // Settings.Login.BackgroundImageUrl;
        }

        /// <summary>
        /// Gets the login page URL.
        /// </summary>
        /// <returns></returns>
        protected string GetLoginPageUrl()
        {
            string loginPage = Client.Site.LoginPage;
            if (string.IsNullOrEmpty(loginPage))
            {
                return "/_bot";
            }
            return loginPage;
        }

        /// <summary>
        /// </summary>
        protected virtual void LoggedIn()
        {
            User user = Sitecore.Security.Accounts.User.FromName(this.fullUserName, false);
            State.Client.UsesBrowserWindows = true;
            LoggedInArgs loggedInArg = new LoggedInArgs()
            {
                Username = this.fullUserName,
                StartUrl = this.startUrl
            };

            LoggedInArgs loggedInArg1 = loggedInArg;

            Pipeline.Start("loggedin", loggedInArg1);

            this.startUrl = loggedInArg1.StartUrl + "&code=" + tokenService.TokenEncoder(user.Name, new TimeSpan(0, 0, 5));

            using (UserSwitcher userSwitcher = new UserSwitcher(user))
            {
                Log.Audit(this, "Bot-auth", new string[0]);
            }
        }

        /// <summary>
        /// </summary>
        protected virtual bool LoggingIn()
        {
            if (string.IsNullOrWhiteSpace(this.UserName.Text))
            {
                return false;
            }
            this.fullUserName = WebUtil.HandleFullUserName(this.UserName.Text);
            this.startUrl = Controllers.BotAuthenticationController.GetTicketCallback(WebUtil.GetQueryString("ticket"));
            this.FailureHolder.Visible = false;
            this.SuccessHolder.Visible = false;

            LoggingInArgs loggingInArg = new LoggingInArgs()
            {
                Username = this.fullUserName,
                Password = this.Password.Text,
                StartUrl = this.startUrl
            };

            Pipeline.Start("loggingin", loggingInArg);
            if ((UIUtil.IsIE() ? true : UIUtil.IsIE11()) && !Regex.IsMatch(WebUtil.GetHostName(), Settings.HostNameValidationPattern, RegexOptions.ECMAScript))
            {
                this.RenderError(Translate.Text("Your login attempt was not successful because the URL hostname contains invalid character(s) that are not recognized by IE. Please check the URL hostname or try another browser."));
                return false;
            }

            if (loggingInArg.Success)
            {
                this.startUrl = loggingInArg.StartUrl;
                return true;
            }

            Log.Audit(string.Format("Login failed: {0}.", loggingInArg.Username), this);
            if (!string.IsNullOrEmpty(loggingInArg.Message))
            {
                this.RenderError(Translate.Text(StringUtil.RemoveLineFeeds(loggingInArg.Message)));
            }

            return false;
        }

        /// <summary>
        /// </summary>
        protected virtual bool Login()
        {
            if (Sitecore.Security.Authentication.AuthenticationManager.Login(this.fullUserName, this.Password.Text, false))
            {
                return true;
            }

            this.RenderError("Your login attempt was not successful. Please try again.");
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void LoginClicked(object sender, EventArgs e)
        {
            if (!this.LoggingIn())
            {
                return;
            }
            if (!this.Login())
            {
                return;
            }

            this.LoggedIn();

            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadString(new Uri(this.startUrl));
                }
            }
            catch (Exception ex)
            {
            }

            this.FormHolder.Visible = false;
            this.SuccessHolder.Visible = true;
            this.SuccessText.Text = "Login succeeded! You can close this tab, please continue in the chat!";
        }

        /// <summary>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            this.DataBind();

            this.LoginForm.Attributes.Add("autocomplete", "off");

            base.OnInit(e);
        }

        private void RenderError(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            this.FailureHolder.Visible = true;
            this.FailureText.Text = text;
        }
    }
}