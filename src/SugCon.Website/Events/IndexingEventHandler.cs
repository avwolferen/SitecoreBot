namespace SugCon.Website.Events
{
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Utilities;
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.Jobs;
    using SugCon.Models.Notifications;
    using System;
    using System.Globalization;
    using System.Linq;

    public class IndexingEventHandler
    {
        public IndexingEventHandler()
        {
        }

        public void Started(object sender, EventArgs args)
        {
            Assert.IsNotNull(args, nameof(args));

            try
            {
                SitecoreEventArgs jobargs = args as SitecoreEventArgs;

                string index_name = jobargs.Parameters[0] as string;
                var index = ContentSearchManager.Indexes.SingleOrDefault(idx => idx.Name == index_name);

                var payload = new IndexingUpdate
                {
                    IndexName = index.Name,
                    State = State.Started,
                    IndexRebuildTime = BuildTime(index),
                    IndexRebuildMilliseconds = IndexHealthHelper.GetIndexRebuildTime(index.Name),
                    ThroughPut = ThroughPut(index),
                    NumberOfDocuments = index.Summary.NumberOfDocuments
                };

                new Services.NotificationService<IndexingUpdate>().Send(payload);
            }
            catch (Exception)
            {
            }
        }

        public void Ended(object sender, EventArgs args)
        {
            Assert.IsNotNull(args, nameof(args));

            try
            {
                SitecoreEventArgs jobargs = args as SitecoreEventArgs;
                string index_name = jobargs.Parameters[0] as string;
                var index = ContentSearchManager.Indexes.SingleOrDefault(idx => idx.Name == index_name);

                var payload = new IndexingUpdate
                {
                    IndexName = index.Name,
                    State = State.Ended,
                    IndexRebuildTime = BuildTime(index),
                    IndexRebuildMilliseconds = IndexHealthHelper.GetIndexRebuildTime(index.Name),
                    ThroughPut = ThroughPut(index),
                    NumberOfDocuments = index.Summary.NumberOfDocuments
                };

                new Services.NotificationService<IndexingUpdate>().Send(payload);
            }
            catch (Exception)
            {
            }
        }

        public static string ThroughPut(ISearchIndex index)
        {
            string throughput = string.Format("<p> <strong>Approximate Throughput: </strong> 0 items per second</p>");
            if (index.Summary.NumberOfDocuments > (long)0 && IndexHealthHelper.GetIndexRebuildTime(index.Name) > 0)
            {
                int indexRebuildTime = IndexHealthHelper.GetIndexRebuildTime(index.Name) / 1000;
                if (indexRebuildTime > 0)
                {
                    long num = index.Summary.NumberOfDocuments / (long)indexRebuildTime;
                    if (num > (long)0)
                    {
                        double num1 = (double)num;
                        throughput = string.Format("<p> <strong>Approximate Throughput: </strong> {0} items per second</p>", num1.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }

            return throughput;
        }

        public static string BuildTime(ISearchIndex index)
        {
            int indexRebuildTime = IndexHealthHelper.GetIndexRebuildTime(index.Name);
            if (indexRebuildTime == 0)
            {
                return " Never Run ";
            }
            int num = indexRebuildTime / 1000;
            if (num < 120)
            {
                return string.Concat(indexRebuildTime / 1000, " seconds");
            }
            if (num >= 120 && num < 3600)
            {
                return string.Concat(indexRebuildTime / 1000 / 60, " minutes");
            }
            if (num < 3600)
            {
                return string.Concat(indexRebuildTime / 1000, " seconds");
            }
            return string.Concat(indexRebuildTime / 1000 / 60 / 60, " hours");
        }
    }
}