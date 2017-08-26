namespace SugCon.SitecoreBot.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using SugCon.SitecoreBot.Constants;
    using SugCon.SitecoreBot.ExtensionMethods;
    using SugCon.SitecoreBot.Services;
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class RootDialog
    {
        [NonSerialized]
        private SitecoreUserManagementAPI _sitecore;

        public SitecoreUserManagementAPI Sitecore(string access_token)
        {
            return _sitecore ?? (_sitecore = SitecoreUserManagementAPI.Instance(access_token));
        }

        [LuisIntent(Intents.List_Users)]
        public async Task ListUser(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (context.IsAdministrator())
            {
                EntityRecommendation domain;
                EntityRecommendation role;
                result.TryFindEntity(Entities.Domain, out domain);
                result.TryFindEntity(Entities.Role, out role);

                await context.PostAsync(context.CreateTypingActivity());

                var users = await SitecoreUserManagementAPI.Instance(context.AccessToken()).GetUsers(domain == null ? null : domain.Entity, role == null ? null : role.Entity);

                StringBuilder sb = new StringBuilder();

                if (users.Count == 0)
                {
                    if (domain != null && role != null)
                    {
                        sb.AppendLine($"I'm sorry, we don't have {role.Entity} users in the {domain.Entity} domain  \n  \n");
                    }
                    else if (domain != null && role == null)
                    {
                        sb.AppendLine($"I'm sorry, we don't have {domain.Entity} domain  \n  \n");
                    }
                    else
                    {
                        sb.AppendLine($"I'm sorry, I haven't found any Sitecore users  \n  \n");
                    }
                }
                else
                {
                    if (domain != null && role != null)
                    {
                        sb.AppendLine($"Here is the list of {role.Entity} users in the {domain.Entity} domain  \n  \n");
                    }
                    else if (domain != null && role == null)
                    {
                        sb.AppendLine($"Here is the list of users in the {domain.Entity} domain  \n  \n");
                    }
                    else
                    {
                        sb.AppendLine($"Here is the list of Sitecore users  \n  \n");
                    }

                    users.ForEach(user =>
                    {
                        if (user.IsOnline)
                        {
                            sb.AppendLine($"{user.FullName} ({user.UserName}) is currently online!  \n");
                        }
                        else
                        {
                            sb.AppendLine(
                                user.LastActivity.HasValue
                                ? $"{user.FullName} ({user.UserName}) last activity {user.LastActivity.Value}  \n"
                                : $"{user.FullName} ({user.UserName})  \n");
                        }
                    });
                }

                await context.PostAsync(sb.ToString());
            }
        }

        [LuisIntent(Intents.Create_User)]
        public async Task CreateUser(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (context.IsAdministrator())
            {
                var model = new Models.Forms.CreateUser();
                EntityRecommendation emailaddress;
                if (result.TryFindEntity(Entities.BuiltIn_Email, out emailaddress))
                {
                    model.EmailAddress = emailaddress.Entity.ToLowerInvariant();

                    var suggestedUsername = model.EmailAddress.Split('@')[0];
                    var exists = await Sitecore(context.AccessToken()).UserExists(suggestedUsername);
                    if (!exists)
                    {
                        model.UserName = suggestedUsername;
                    }
                }

                var createUserForm = new FormDialog<Models.Forms.CreateUser>(model, Models.Forms.CreateUser.BuildForm, FormOptions.PromptInStart);

                context.Call(createUserForm, CreateUser_Callback);
            }
        }

        private async Task CreateUser_Callback(IDialogContext context, IAwaitable<Models.Forms.CreateUser> result)
        {
            var create = await result;

            var response = await Sitecore(context.GetProfile().AccessToken).CreateUser(create);

            if (response.Success)
            {
                await context.PostAsync($"{create.FullName} ({create.UserName}) has been created.");
            }
            else
            {
                await context.PostAsync($"{create.UserName} couldn't be created because {response.Message}.");
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent(Intents.Disable_User)]
        public async Task DisableUser(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            EntityRecommendation username;

            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (!result.TryFindEntity(Entities.Username, out username))
            {
                await context.PostAsync("I'm sorry I really don't have a clue which user you mean.");
            }
            else if (context.IsAdministrator())
            {
                var user = await Sitecore(context.GetProfile().AccessToken).GetProfile(username.Entity);
                if (user == null)
                {
                    await context.PostAsync($"There doesn't exist a user with username '{username.Entity}'");
                }
                else
                {
                    var profile = await Sitecore(context.GetProfile().AccessToken).GetProfile(username.Entity);
                    if (!profile.Success)
                    {
                        context.PrivateConversationData.SetValue(ContextConstants.ActionOnUserKey, username.Entity);
                        PromptDialog.Confirm(context, DisableUser_Callback, $"Are you sure you would like to disable {user.FullName} ({username.Entity})", "Please, make a choice.");
                    }
                }
            }
        }

        private async Task DisableUser_Callback(IDialogContext context, IAwaitable<bool> result)
        {
            var confirmed = await result;

            string username;
            if (confirmed && context.PrivateConversationData.TryGetValue(ContextConstants.ActionOnUserKey, out username) && !string.IsNullOrWhiteSpace(username))
            {
                var user = await Sitecore(context.GetProfile().AccessToken).GetProfile(username);
                var response = await Sitecore(context.GetProfile().AccessToken).DisableUser(username);
                if (response.Success)
                {
                    await context.PostAsync($"{user.FullName} ({username}) has been disabled.");
                }
                else
                {
                    await context.PostAsync($"{user.FullName} ({username}) couldn't be disabled because {response.Message}.");
                }
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent(Intents.Enable_User)]
        public async Task EnableUser(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            EntityRecommendation username;

            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (!result.TryFindEntity(Entities.Username, out username))
            {
                await context.PostAsync("I'm sorry I really don't have a clue which user you mean.");
            }
            else if (context.IsAdministrator())
            {
                context.PrivateConversationData.SetValue(ContextConstants.ActionOnUserKey, username.Entity);

                var user = await Sitecore(context.GetProfile().AccessToken).GetProfile(username.Entity);
                if (user == null)
                {
                    await context.PostAsync($"There doesn't exist a user with username '{username.Entity}'");
                }
                else
                {
                    PromptDialog.Confirm(context, EnableUser_Callback, $"Are you sure you would like to enable {user.FullName} ({username.Entity})", "Please, make a choice.");
                }
            }
        }

        private async Task EnableUser_Callback(IDialogContext context, IAwaitable<bool> result)
        {
            var confirmed = await result;

            string username;
            if (confirmed && context.PrivateConversationData.TryGetValue(ContextConstants.ActionOnUserKey, out username) && !string.IsNullOrWhiteSpace(username))
            {
                var user = await Sitecore(context.GetProfile().AccessToken).GetProfile(username);
                var response = await Sitecore(context.GetProfile().AccessToken).EnableUser(username);
                if (response.Success)
                {
                    await context.PostAsync($"{user.FullName} ({username}) has been enabled.");
                }
                else
                {
                    await context.PostAsync($"{user.FullName} ({username}) couldn't be enabled because {response.Message}.");
                }
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent(Intents.Delete_User)]
        public async Task DeleteUser(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            EntityRecommendation username;

            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
            else if (!result.TryFindEntity(Entities.Username, out username))
            {
                await context.PostAsync("I'm sorry I really don't have a clue which user you mean.");
            }
            else if (context.IsAdministrator())
            {
                context.PrivateConversationData.SetValue(ContextConstants.ActionOnUserKey, username.Entity);
                var user = await Sitecore(context.GetProfile().AccessToken).GetProfile(username.Entity);
                if (user == null)
                {
                    await context.PostAsync($"There doesn't exist a user with username '{username.Entity}'");
                }
                else
                {
                    PromptDialog.Confirm(context, DeleteUser_Callback, $"Are you sure you would like to delete '{user.FullName} ({username.Entity})'", "Please, make a choice.");
                }
            }
        }

        private async Task DeleteUser_Callback(IDialogContext context, IAwaitable<bool> result)
        {
            var confirmed = await result;

            string username;
            if (confirmed && context.PrivateConversationData.TryGetValue(ContextConstants.ActionOnUserKey, out username) && !string.IsNullOrWhiteSpace(username))
            {
                var user = await Sitecore(context.GetProfile().AccessToken).GetProfile(username);
                var response = await Sitecore(context.GetProfile().AccessToken).DeleteUser(username);
                if (response.Success)
                {
                    await context.PostAsync($"{user.FullName} ({username}) has been deleted.");
                }
                else
                {
                    await context.PostAsync($"{user.FullName} ({username}) couldn't be deleted because you {response.Message}.");
                }
            }

            context.Wait(MessageReceived);
        }
    }
}