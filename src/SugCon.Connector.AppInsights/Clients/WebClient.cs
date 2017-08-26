using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugCon.Connector.AppInsights.Clients
{
    public class WebClient : System.Net.WebClient
    {
        public WebClient(string apiKey) : base()
        {
            Headers.Add("X-Api-Key", apiKey);
        }
    }
}
