using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugCon.Connector.AppInsights.Models
{
    [JsonObject("value")]
    public class MetricIntervalResult
    {
        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }

        [JsonProperty("interval")]
        public string Interval { get; set; }

        [JsonProperty("segments")]
        public IList<Segment> Segments { get; set; }
    }
}
