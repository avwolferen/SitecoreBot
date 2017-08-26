using SugCon.Connector.AppInsights.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugCon.Connector.AppInsights.ConsoleTest
{
    class Program
    {
        private static IApplicationInsightsService _aiService;
        public static IApplicationInsightsService AIService
        {
            get
            {
                return _aiService ?? (_aiService = new ApplicationInsightsService("DEMO_APP", "DEMO_KEY"));
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("AI Console Debugger");

            var metric = AIService.GetMetric(Metrics.UsersCount);
            Console.WriteLine(metric.Start);

            var metric2 = AIService.GetMetric(Metrics.UsersCount, new TimeSpan(2, 0, 0, 0));
            Console.WriteLine(metric2.Start);

            var metric3 = AIService.GetMetric(Metrics.UsersCount, new TimeSpan(2, 0, 0, 0), new TimeSpan(0, 30, 0));
            Console.WriteLine(metric3.Start);

            //Console.WriteLine(AIService.GetMetric(Metrics.UsersCount, new DateTime(2017, 3, 20), new DateTime(2017, 4, 2)));
            //Console.WriteLine(AIService.GetMetric(Metrics.UsersCount, new DateTime(2017, 3, 20), new DateTime(2017, 4, 2), new TimeSpan(0, 30, 0)));

            Console.ReadLine();
        }
    }
}
