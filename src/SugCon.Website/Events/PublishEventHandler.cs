namespace SugCon.Website.Events
{
    using Sitecore.Events;
    using Sitecore.Publishing;
    using Sitecore.Security.Accounts;
    using SugCon.Models.Notifications;
    using System;

    public class PublishEventHandler
    {

        public PublishEventHandler()
        {
        }

        public void Started(object sender, EventArgs args)
        {
            try
            {
                if (args != null)
                {
                    SitecoreEventArgs sitecoreEventArg = args as SitecoreEventArgs;
                    if (sitecoreEventArg != null)
                    {
                        Publisher parameters = sitecoreEventArg.Parameters[0] as Publisher;
                        if (parameters != null)
                        {
                            try
                            {
                                var publishData = new PublishUpdate
                                {
                                    UserName = parameters.Options.UserName,
                                    CompareRevisions = parameters.Options.CompareRevisions,
                                    Deep = parameters.Options.Deep,
                                    FromDate = parameters.Options.FromDate,
                                    Language = parameters.Options.Language.Name,
                                    Mode = parameters.Options.Mode.ToString(),
                                    PublishDate = parameters.Options.PublishDate,
                                    PublishingTargets = parameters.Options.PublishingTargets,
                                    PublishRelatedItems = parameters.Options.PublishRelatedItems,
                                    RecoveryId = parameters.Options.RecoveryId,
                                    RepublishAll = parameters.Options.RepublishAll,
                                    RootItemPath = parameters.Options.RootItem?.Paths.FullPath,
                                    State = State.Started
                                };

                                User user = User.FromName(parameters.Options.UserName, false);
                                if (user != null)
                                {
                                    publishData.FullName = user.Profile.FullName;
                                }

                                new Services.NotificationService<PublishUpdate>().Send(publishData);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
            }
        }

        public void Ended(object sender, EventArgs args)
        {
            try
            {
                if (args != null)
                {
                    SitecoreEventArgs sitecoreEventArg = args as SitecoreEventArgs;
                    if (sitecoreEventArg != null)
                    {
                        Publisher parameters = sitecoreEventArg.Parameters[0] as Publisher;
                        if (parameters != null)
                        {
                            try
                            {
                                var publishData = new PublishUpdate
                                {
                                    UserName = parameters.Options.UserName,
                                    CompareRevisions = parameters.Options.CompareRevisions,
                                    Deep = parameters.Options.Deep,
                                    FromDate = parameters.Options.FromDate,
                                    Language = parameters.Options.Language.Name,
                                    Mode = parameters.Options.Mode.ToString(),
                                    PublishDate = parameters.Options.PublishDate,
                                    PublishingTargets = parameters.Options.PublishingTargets,
                                    PublishRelatedItems = parameters.Options.PublishRelatedItems,
                                    RecoveryId = parameters.Options.RecoveryId,
                                    RepublishAll = parameters.Options.RepublishAll,
                                    RootItemPath = parameters.Options.RootItem?.Paths.FullPath,
                                    State = State.Ended
                                };

                                User user = User.FromName(parameters.Options.UserName, false);
                                if (user != null)
                                {
                                    publishData.FullName = user.Profile.FullName;
                                }

                                new Services.NotificationService<PublishUpdate>().Send(publishData);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
            }
        }
    }
}