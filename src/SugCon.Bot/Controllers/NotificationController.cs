namespace SugCon.SitecoreBot.Controllers
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using SugCon.Models.Notifications;
    using SugCon.Models.Publishing;
    using SugCon.SitecoreBot.Services;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class NotificationController : ApiController
    {
        public static IDictionary<string, ConversationReference> SubscribedPublishConversations = new Dictionary<string, ConversationReference>();
        public static IDictionary<string, ConversationReference> SubscribedIndexingConversations = new Dictionary<string, ConversationReference>();

        [HttpPost]
        [Route("api/notify/publish")]
        public async Task<HttpResponseMessage> PublishNotification([FromBody] PublishUpdate data)
        {
            foreach (var conversation in SubscribedPublishConversations.Keys)
            {
                var conversationReference = SubscribedPublishConversations[conversation];

                IMessageActivity message = Activity.CreateMessageActivity();
                message.ChannelId = conversationReference.ChannelId;
                message.From = conversationReference.Bot;
                message.Recipient = conversationReference.User;
                message.Conversation = conversationReference.Conversation;
                message.Locale = "en-US";

                if (data.State == State.Ended)
                {
                    message.Text = $"Publish completed, was started by {data.FullName} ({data.UserName})";
                }
                else if (data.State == State.Started)
                {
                    if (data.Mode == "Full" && data.CompareRevisions)
                    {
                        message.Text = $"{data.FullName} ({data.UserName}) just started a smart publish (publish differences between source and target database)";
                    }
                    else if (data.Mode == "Full" && !data.CompareRevisions)
                    {
                        message.Text = $"{data.FullName} ({data.UserName}) just started a republish (publish everything)";
                    }
                    else if (data.Mode == "Incremental")
                    {
                        message.Text = $"{data.FullName} ({data.UserName}) just started a incremental publish (publish only changed items)";
                    }
                    else if (data.Mode == "SingleItem" && data.CompareRevisions && data.Deep)
                    {
                        message.Text = $"{data.FullName} ({data.UserName}) just started a smart publish (publish only changed items) for everything under {data.RootItemPath}";
                    }
                    else if (data.Mode == "SingleItem" && data.CompareRevisions && !data.Deep)
                    {
                        message.Text = $"{data.FullName} ({data.UserName}) just started a smart publish (publish only changed items) only for {data.RootItemPath}";
                    }
                    else if (data.Mode == "SingleItem" && !data.CompareRevisions && data.Deep)
                    {
                        message.Text = $"{data.FullName} ({data.UserName}) just started a republish (publish everything) for everything under {data.RootItemPath}";
                    }
                    else if (data.Mode == "SingleItem" && !data.CompareRevisions && !data.Deep)
                    {
                        message.Text = $"{data.FullName} ({data.UserName}) just started a republish (publish everything) only for {data.RootItemPath}";
                    }
                    else
                    {
                        message.Text = $"{data.FullName} ({data.UserName}) just started a publish for {data.RootItemPath}";
                    }
                }

                var connector = new ConnectorClient(new Uri(conversationReference.ServiceUrl));
                await connector.Conversations.SendToConversationAsync((Activity)message);
            }

            return Request.CreateResponse("OK");
        }

        [HttpPost]
        [Route("api/notify/index")]
        public async Task<HttpResponseMessage> IndexNotification([FromBody] IndexingUpdate data)
        {
            foreach (var conversation in SubscribedIndexingConversations.Keys)
            {
                var conversationReference = SubscribedIndexingConversations[conversation];

                IMessageActivity message = Activity.CreateMessageActivity();
                message.ChannelId = conversationReference.ChannelId;
                message.From = conversationReference.Bot;
                message.Recipient = conversationReference.User;
                message.Conversation = conversationReference.Conversation;
                message.Locale = "en-US";

                switch (data.State)
                {
                    case State.Started:
                        message.Text = data.IndexRebuildMilliseconds == 0 ? $"Index rebuild for {data.IndexName} started" : $"Index rebuild for {data.IndexName} started, will take approximately {data.IndexRebuildTime}";
                        break;
                    case State.Ended:
                        message.Text = $"Index rebuild for {data.IndexName} completed in {data.IndexRebuildTime}";
                        break;
                    default:
                        break;
                }

                var connector = new ConnectorClient(new Uri(conversationReference.ServiceUrl));
                await connector.Conversations.SendToConversationAsync((Activity)message);
            }

            return Request.CreateResponse("OK");
        }

        public static void SubscribeToIndexingUpdates(IDialogContext context)
        {
            if (!SubscribedIndexingConversations.ContainsKey(context.Activity.Conversation.Id))
            {
                var conversationReference = new ConversationReference(
                    user: new ChannelAccount(id: context.Activity.From.Id),
                    conversation: new ConversationAccount(id: context.Activity.Conversation.Id),
                    bot: new ChannelAccount(id: context.Activity.Recipient.Id),
                    channelId: context.Activity.ChannelId,
                    serviceUrl: context.Activity.ServiceUrl);

                SubscribedIndexingConversations.Add(conversationReference.Conversation.Id, conversationReference);
            }
        }

        public static void UnSubscribeFromIndexingUpdates(IDialogContext context)
        {
            if (SubscribedIndexingConversations.ContainsKey(context.Activity.Conversation.Id))
            {
                SubscribedIndexingConversations.Remove(context.Activity.Conversation.Id);
            }
        }

        public static void SubscribeToPublishUpdates(IDialogContext context)
        {
            if (!SubscribedPublishConversations.ContainsKey(context.Activity.Conversation.Id))
            {
                var conversationReference = new ConversationReference(
                    user: new ChannelAccount(id: context.Activity.From.Id),
                    conversation: new ConversationAccount(id: context.Activity.Conversation.Id),
                    bot: new ChannelAccount(id: context.Activity.Recipient.Id),
                    channelId: context.Activity.ChannelId,
                    serviceUrl: context.Activity.ServiceUrl);

                SubscribedPublishConversations.Add(conversationReference.Conversation.Id, conversationReference);
            }
        }

        public static void UnSubscribeToPublishUpdates(IDialogContext context)
        {
            if (SubscribedPublishConversations.ContainsKey(context.Activity.Conversation.Id))
            {
                SubscribedPublishConversations.Remove(context.Activity.Conversation.Id);
            }
        }
    }
}