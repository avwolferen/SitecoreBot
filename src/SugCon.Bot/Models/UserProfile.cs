using SugCon.Models.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SugCon.SitecoreBot.Models
{
    [Serializable]
    public class UserProfile
    {
        public UserProfile()
        {
            AccessToken = string.Empty;
            FullName = string.Empty;
            Login = string.Empty;
            Roles = new List<string>();
            ApplicationInsights = new ApplicationInsights();
        }

        public string FullName { get; set; }

        public string Login { get; set; }

        public string AccessToken { get; set; }

        public List<string> Roles { get; set; }

        public bool IsAdministrator { get; set; }

        public ApplicationInsights ApplicationInsights { get; set; }
    }
}