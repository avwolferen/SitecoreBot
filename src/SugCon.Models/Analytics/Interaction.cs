namespace SugCon.Models.Analytics
{
    using Sitecore.Analytics.Model;
    using System;

    public class Interaction
    {
        public string UserAgent { get; set; }
        public int TrafficType { get; set; }
        public string SiteName { get; set; }
        //     c virtual string ReferringSite { get; set; }
        public string Referrer { get; set; }

        //       public virtual List<PageData> Pages { get; set; }

        public byte[] Ip { get; set; }
        public int ContactVisitIndex { get; set; }
        public ScreenData Screen { get; set; }
        public BrowserData Browser { get; set; }
        public int Value { get; set; }
        public int VisitPageCount { get; set; }

        public Guid InteractionId { get; set; }
        public Guid ContactId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime SaveDateTime { get; set; }
        public Guid? CampaignId { get; set; }
        public Guid ChannelId { get; set; }
        public Guid? VenueId { get; set; }
    }
}