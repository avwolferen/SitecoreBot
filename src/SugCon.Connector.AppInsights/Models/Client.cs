namespace SugCon.Connector.AppInsights.Models
{
    using System;

    [Serializable]
    public class Client
    {
        public string model;
        public string os;
        public string type;
        public string ip;
        public string city;
        public string stateOrProvince;
        public string countryOrRegion;
    }
}