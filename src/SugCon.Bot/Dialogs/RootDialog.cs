namespace SugCon.SitecoreBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using SugCon.SitecoreBot.ExtensionMethods;
    using System.Threading;
    using Microsoft.Bot.Builder.Internals.Fibers;

    /// <summary>
    /// Update the model references with the imported model
    /// </summary>
    [LuisModel("d7df2d3f-c5b7-4f5a-8173-4d07f32c4c39", "20a2174d96fe48949d498024d9ec2351")]
    [Serializable]
    public partial class RootDialog : LuisDialog<object>
    {
        [LuisIntent(Constants.Intents.SignOut)]
        public async Task SignOut(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                await context.PostAsync("Uhhhh, okay! I already have forgotten you!");
            }
            else
            {
                var profile = context.GetProfile();
                PromptDialog.Confirm(context, SignOut_Callback, $"{profile.FullName}, are you sure you would like to signout?", "Please, make a choice.");
            }
        }

        private async Task SignOut_Callback(IDialogContext context, IAwaitable<bool> result)
        {
            bool signout = await result;
            if (signout)
            {
                context.RemoveProfile();
                await context.PostAsync("You're a wise person!");
            }
            else
            {
                await context.PostAsync("Never mind! Let's Bot on!");
            }
        }

        [LuisIntent(Constants.Intents.SignIn)]
        public async Task SignIn(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (!isAuthenticated)
            {
                context.SetUnauthorizedMessageText(result.Query);
                await context.Forward(new AuthenticationDialog(), this.ResumeAfterAuth, context.MakeMessage(), CancellationToken.None);
            }
        }

        [LuisIntent(Constants.Intents.Status)]
        public async Task Status(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Peace man! If I aint't talking to you I sure was having a problem. Let me just check some stuff to make sure everything is set up correct.");
            await context.PostAsync(context.CreateTypingActivity());
            Thread.Sleep(4000);
            await context.PostAsync("I haven't found any problems so far! Now that we're all set, what would you like to do now?");
        }

        [LuisIntent(Constants.Intents.Greeting)]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            var isAuthenticated = await context.IsAuthenticated();
            if (isAuthenticated)
            {
                var profile = context.GetProfile();
                await context.PostAsync($"{profile.FullName}, want some coffee? Hold on!");
                await context.PostAsync(context.CreateTypingActivity());
                Thread.Sleep(1500);
            }

            int random = new Random(DateTime.Now.Second).Next(0, 4);
            switch (random)
            {
                case 1:
                    await context.PostAsync($"Yes, tell me! What are you looking for?");
                    break;
                case 2:
                    await context.PostAsync($"Hello from the other side!");
                    break;
                case 3:
                    await context.PostAsync($"Bots are just \"fake\" technology!");
                    break;
                case 4:
                    await context.PostAsync($"Peace man!");
                    break;
                default:
                    await context.PostAsync($"Yes, I'm listening...");
                    break;
            }
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry, I can't understand what you mean.");
        }

        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            var profile = context.GetProfile();

            await context.PostAsync($"Welcome back {profile.FullName}!");
            context.Wait(MessageReceived);
        }
    }
}