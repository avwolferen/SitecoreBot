namespace SugCon.Connector.AppInsights.Models
{
    using System;

    [Serializable]
    public class Trace
    {
        public Guid id;
        public string type;
        public DateTime timestamp;

        public CustomDimensions customDimensions;
        public string customMeasurements;

        public TraceMessage trace;
        public Client client;
    }
}