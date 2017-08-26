namespace SugCon.Models.UserManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum UserRole
    {
        User,
        Analytics,
        Bot
    }

    public class CreateUserRequest
    {
        public string EmailAddress;
        public string FullName;
        public string UserName;
        public List<UserRole> UserRole;
        public bool AdministratorRoleForUser;
        public bool EmailSendWithPasswordToTheUser;
    }
}
