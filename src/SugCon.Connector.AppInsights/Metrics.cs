using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugCon.Connector.AppInsights
{
    public static class Metrics
    {
        public static string RequestsCount = "requests/count";
        public static string RequestsDuration = "requests/duration";
        public static string RequestsFailed = "requests/failed";
        public static string PageViewsCount = "pageViews/count";
        public static string PageViewsDuration = "pageViews/duration";
        public static string BrowserTimingsNetworkDuration = "browserTimings/networkDuration";
        public static string BrowserTimingsSendDuration = "browserTimings/sendDuration";
        public static string BrowserTimingsReceiveDuration = "browserTimings/receiveDuration";
        public static string BrowserTimingsProcessingDuration = "browserTimings/processingDuration";
        public static string BrowserTimingsTotalDuration = "browserTimings/totalDuration";
        public static string UsersCount = "users/count";
        public static string UsersAuthenticated = "users/authenticated";
        public static string SessionsCount = "sessions/count";
        public static string CustomEventsCount = "customEvents/count";
        public static string DependenciesCount = "dependencies/count";
        public static string DependenciesFailed = "dependencies/failed";
        public static string DependenciesDuration = "dependencies/duration";
        public static string ExceptionsCount = "exceptions/count";
        public static string ExceptionsBrowser = "exceptions/browser";
        public static string ExceptionsServer = "exceptions/server";
        public static string AvailabilityResultsCount = "availabilityResults/count";
        public static string AvailabilityResultsDuration = "availabilityResults/duration";
    }
}
