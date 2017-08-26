namespace SugCon.Connector.AppInsights
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SugCon.Connector.AppInsights.ExtensionMethods;
    using SugCon.Connector.AppInsights.Interfaces;
    using SugCon.Connector.AppInsights.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ApplicationInsightsService : IApplicationInsightsService
    {
        public ApplicationInsightsService(string applicationID, string apiKey)
        {
            this.applicationId = applicationID;
            this.apiKey = apiKey;
        }

        private readonly string applicationId;
        private readonly string apiKey;

        private string baseUrl
        {
            get
            {
                return $"https://api.applicationinsights.io/beta/apps/{applicationId}";
            }
        }

        public string GetMetricsMetaData()
        {
            using (var client = new Clients.WebClient(apiKey))
            {
                return client.DownloadString(new Uri($"{baseUrl}/metrics/metadata"));
            }
        }

        public MetricResult GetMetric(string metricId)
        {
            return GetMetric(metricId, new TimeSpan(12, 0, 0));
        }

        public MetricResult GetMetric(string metricId, TimeSpan timespan)
        {
            using (var client = new Clients.WebClient(apiKey))
            {
                JObject jsonResponse = JObject.Parse(client.DownloadString(new Uri($"{baseUrl}/metrics/{metricId}?timespan={timespan.ToApiTimeSpan()}")));

                return JsonConvert.DeserializeObject<MetricResult>(jsonResponse["value"].ToString());
            }
        }

        public MetricIntervalResult GetMetric(string metricId, TimeSpan timespan, TimeSpan interval)
        {
            using (var client = new Clients.WebClient(apiKey))
            {
                JObject jsonResponse = JObject.Parse(client.DownloadString(new Uri($"{baseUrl}/metrics/{metricId}?timespan={timespan.ToApiTimeSpan()}&interval={interval.ToApiTimeSpan()}")));

                return JsonConvert.DeserializeObject<MetricIntervalResult>(jsonResponse["value"].ToString());
            }
        }

        public MetricResult GetMetric(string metricId, DateTime fromDate, DateTime toDate)
        {
            using (var client = new Clients.WebClient(apiKey))
            {
                JObject jsonResponse = JObject.Parse(client.DownloadString(new Uri($"{baseUrl}/metrics/{metricId}?timespan={fromDate.ToApiTimeSpan(toDate)}")));

                return JsonConvert.DeserializeObject<MetricResult>(jsonResponse["value"].ToString());
            }
        }

        public MetricResult GetMetric(string metricId, DateTime fromDate, TimeSpan interval)
        {
            using (var client = new Clients.WebClient(apiKey))
            {
                JObject jsonResponse = JObject.Parse(client.DownloadString(new Uri($"{baseUrl}/metrics/{metricId}?timespan={fromDate.ToApiTimeSpan()}&interval={interval.ToApiTimeSpan()}")));

                return JsonConvert.DeserializeObject<MetricResult>(jsonResponse["value"].ToString());
            }
        }

        public MetricIntervalResult GetMetric(string metricId, DateTime fromDate, DateTime toDate, TimeSpan interval)
        {
            using (var client = new Clients.WebClient(apiKey))
            {
                JObject jsonResponse = JObject.Parse(client.DownloadString(new Uri($"{baseUrl}/metrics/{metricId}?timespan={fromDate.ToApiTimeSpan(toDate)}&interval={interval.ToApiTimeSpan()}")));

                return JsonConvert.DeserializeObject<MetricIntervalResult>(jsonResponse["value"].ToString());
            }
        }

        public IList<Trace> GetTraces(TimeSpan timespan, int top)
        {
            using (var client = new Clients.WebClient(apiKey))
            {
                JObject jsonResponse = JObject.Parse(client.DownloadString(new Uri($"{baseUrl}/events/traces?timespan={timespan.ToApiTimeSpan()}&$top={top}")));

                IList<Trace> traces = JsonConvert.DeserializeObject<List<Trace>>(jsonResponse["value"].ToString());

                return traces.Reverse().ToList();
            }
        }
    }
}
