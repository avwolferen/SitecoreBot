namespace SugCon.SitecoreBot.Models.Forms
{
    using Microsoft.Bot.Builder.FormFlow;
    using SugCon.SitecoreBot.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public enum UserRole
    {
        User,
        Analytics,
        Bot,
        BotFramework
    }

    [Serializable]
    public class CreateUser
    {
        [Pattern(@"^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$")]
        public string EmailAddress;

        public string FullName;

        [Pattern(@"^[_a-zA-Z0-9]+$")]
        public string UserName;

        public List<UserRole> UserRoles;

        public bool? AdministratorRoleForUser;

        public bool? EmailSendWithPasswordToTheUser;

        public static IForm<CreateUser> BuildForm()
        {
            var form = new FormBuilder<CreateUser>()
                .Message("Let's get some more details for the user!")
                .Field(nameof(FullName))
                .Field(nameof(UserName), validate: async (state, value) =>
                {
                    var exists = await SitecoreUserManagementAPI.Instance().UserExists((string)value);
                    return new ValidateResult { IsValid = !exists, Value = value, Feedback = exists ? $"Username {value} is already taken." : $"Lucky you! Username {value} is available!" };
                })
                .Field(nameof(EmailAddress))
                .Confirm("Is the emailaddress {EmailAddress} correct?")
                //.Field(nameof(IsAdministrator), prompt: "Do you want this user to be an administrator?")
                .AddRemainingFields()
                .Confirm("Do you want to create a new user for {FullName} ({UserName})?");

            form.Configuration.DefaultPrompt.ChoiceStyle = ChoiceStyleOptions.Buttons;

            return form.Build();
        }
    }
}