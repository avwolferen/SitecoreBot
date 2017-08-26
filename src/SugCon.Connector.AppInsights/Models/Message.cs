namespace SugCon.Connector.AppInsights.Models
{
    using System;
    [Serializable]
    public class TraceMessage
    {
        public string message;
        public int severityLevel;
    }
}