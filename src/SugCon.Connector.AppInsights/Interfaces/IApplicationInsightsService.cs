using SugCon.Connector.AppInsights.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugCon.Connector.AppInsights.Interfaces
{
    public interface IApplicationInsightsService
    {
        string GetMetricsMetaData();

        MetricResult GetMetric(string metricId);

        MetricResult GetMetric(string metricId, TimeSpan timespan);

        IList<Trace> GetTraces(TimeSpan timespan, int top);

        MetricIntervalResult GetMetric(string metricId, TimeSpan timespan, TimeSpan interval);

        MetricResult GetMetric(string metricId, DateTime fromDate, DateTime toDate);

        MetricIntervalResult GetMetric(string metricId, DateTime fromDate, DateTime toDate, TimeSpan interval);
    }
}
