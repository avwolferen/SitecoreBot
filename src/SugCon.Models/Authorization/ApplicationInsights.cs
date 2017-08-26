using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugCon.Models.Authorization
{
    [Serializable]
    public class ApplicationInsights
    {
        public string ApplicationId { get; set; }
        public string ApiKey { get; set; }
    }
}
