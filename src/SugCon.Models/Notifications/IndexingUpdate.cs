namespace SugCon.Models.Notifications
{
    using System;

    [Serializable]
    public class IndexingUpdate
    {
        public State State;
        public string UserName;
        public string Message;
        public string IndexName;
        public long NumberOfDocuments;
        public DateTime LastUpdated;
        public string IndexRebuildTime;
        public string Job;
        public string ThroughPut;
        public int IndexRebuildMilliseconds;
    }
}
