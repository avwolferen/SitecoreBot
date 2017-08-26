namespace SugCon.Connector.AppInsights.Models
{
    using System;
    [Serializable]
    public class CustomDimensions
    {
        public string EventId;
        public string InstanceName;
        public string Role;
    }
}