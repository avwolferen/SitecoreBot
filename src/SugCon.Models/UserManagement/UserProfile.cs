using SugCon.Models.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugCon.Models.UserManagement
{
    public class UserProfile
    {
        public UserProfile()
        {
            Roles = new List<string>();
            ApplicationInsights = new ApplicationInsights();
        }

        public string FullName { get; set; }

        public string EmailAddress { get; set; }

        public string UserName { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; }

        public bool IsAdministrator { get; set; }

        public List<string> Roles { get; set; }

        public ApplicationInsights ApplicationInsights { get; set; }

        public DateTime? LastLogin { get; set; }

        public bool IsOnline { get; set; }

        public DateTime? LastActivity { get; set; }
    }
}
