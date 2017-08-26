using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugCon.Models.Notifications
{
    [Serializable]
    public class PublishUpdate
    {
        public List<string> PublishingTargets;
        public string TargetDatabase;
        public string SourceDatabase;
        public string RootItemPath;
        public bool RepublishAll;
        //public Replacer Replacer;
        public DateTime PublishDate;
        public int LanguageGroupingHashCode;
        public string Language;
        public Guid RecoveryId;
        public DateTime ExplicitlySetFromDate;
        public DateTime FromDate;
        public bool Deep;
        //public bool CompareRevisions;
        public bool PublishRelatedItems;
        public string Mode;
        public string UserName;
        public string FullName;
        public State State;
        public bool CompareRevisions;
    }
}
