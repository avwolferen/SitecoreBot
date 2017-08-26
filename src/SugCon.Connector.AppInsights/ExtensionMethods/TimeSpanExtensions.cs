using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugCon.Connector.AppInsights.ExtensionMethods
{
    public static class TimeSpanExtensions
    {
        public static string ToApiTimeSpan(this TimeSpan timeSpan)
        {
            StringBuilder timeSpanBuilder = new StringBuilder("P");

            if (timeSpan.Days > 0)
            {
                timeSpanBuilder.Append($"{timeSpan.Days}D");
            }

            if (timeSpan.Hours > 0)
            {
                timeSpanBuilder.Append($"T{timeSpan.Hours}H");
            }

            if (timeSpan.Minutes > 0)
            {
                timeSpanBuilder.Append($"{timeSpan.Minutes}M");
            }

            return timeSpanBuilder.ToString();            
        }

        public static string ToApiTimeSpan(this DateTime fromDate, DateTime toDate)
        {
            return $"{fromDate.ToString("yyyy-MM-dd")}/{toDate.ToString("yyyy-MM-dd")}";
        }

        public static string ToApiTimeSpan(this DateTime fromDate)
        {
            return fromDate >= DateTime.UtcNow ? string.Empty : $"P{(fromDate - DateTime.UtcNow).TotalDays}";
        }
    }
}
