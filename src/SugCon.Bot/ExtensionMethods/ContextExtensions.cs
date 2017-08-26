using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SugCon.SitecoreBot.ExtensionMethods
{
    public static class ContextExtensions
    {
        public static Models.UserProfile GetProfile(this IDialogContext context)
        {
            Models.UserProfile data;

            context.PrivateConversationData.TryGetValue<Models.UserProfile>(ContextConstants.UserProfileKey, out data);

            return data;
        }

        public static IMessageActivity CreateTypingActivity(this IDialogContext context)
        {
            IMessageActivity typingActivity = context.MakeMessage();
            typingActivity.Type = ActivityTypes.Typing;

            return typingActivity;
        }

        /// <summary>
        /// Get the access token for API access
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string AccessToken(this IDialogContext context)
        {
            return context.GetProfile().AccessToken;
        }

        /// <summary>
        /// Returns true if the user is in role
        /// </summary>
        /// <param name="context"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static bool IsInRole(this IDialogContext context, string role)
        {
            return context.GetProfile().Roles.Contains(role);
        }

        /// <summary>
        /// User is administrator
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsAdministrator(this IDialogContext context)
        {
            return context.GetProfile().IsAdministrator;
        }

        public static void SetProfile(this IDialogContext context, Models.UserProfile profile)
        {
            context.PrivateConversationData.SetValue(ContextConstants.UserProfileKey, profile);
        }

        public static void RemoveProfile(this IDialogContext context)
        {
            context.PrivateConversationData.RemoveValue(ContextConstants.UserProfileKey);
        }

        public async static Task<bool> IsAuthenticated(this IDialogContext context)
        {
            var profile = context.GetProfile();
            return await profile.IsAuthenticated();
        }



        public static void SetUnauthorizedMessageText(this IDialogContext context, string text)
        {
            context.PrivateConversationData.SetValue(ContextConstants.MessageBeforeAuthenticatedKey, text);        
        }

        public static string GetUnauthorizedMessageText(this IDialogContext context)
        {
            string messageText = null;
            if (context.PrivateConversationData.TryGetValue(ContextConstants.MessageBeforeAuthenticatedKey, out messageText))
            {
                return messageText;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}