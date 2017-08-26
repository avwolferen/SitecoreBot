namespace SugCon.Website.Controllers
{
    using Newtonsoft.Json;
    using Sitecore;
    using Sitecore.Diagnostics;
    using Sitecore.Security.Accounts;
    using Sitecore.Security.Domains;
    using SugCon.Models.Authorization;
    using SugCon.Models.UserManagement;
    using SugCon.Website.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Results;
    using System.Web.Security;

    public class BotProfileController : ApiControllerBase
    {
        public BotProfileController() : base(new TokenService())
        {
        }

        public BotProfileController(ITokenService tokenService) : base(tokenService)
        {
        }

        [HttpGet]
        [Route("_bot/api/profile/exists")]
        public IHttpActionResult Exists([FromUri] string username = null)
        {
            if (string.IsNullOrWhiteSpace(AccessToken))
            {
                return Unauthorized();
            }

            if (!IsValidBotSecret(AccessToken.Split('|').First(), AccessToken.Split('|').Last()))
            {
                return Unauthorized();
            }

            bool exists = Sitecore.Security.Accounts.User.Exists(Context.Site.Domain.GetFullName(username));

            return new JsonResult<bool>(exists, new JsonSerializerSettings(), Encoding.UTF8, this);
        }

        [HttpGet]
        [Route("_bot/api/profile/get")]
        public IHttpActionResult GetProfile([FromUri] string username = null)
        {
            if (!IsAuthenticated)
            {
                return Unauthorized();
            }

            using (new UserSwitcher(Context.Site.Domain.GetFullName(this.ApiUser), false))
            {
                if (!Context.User.IsAdministrator)
                {
                    return Unauthorized();
                }

                var response = new Models.UserManagement.UserProfile();

                string name = Context.Site.Domain.GetFullName(username ?? this.ApiUser);
                if (Sitecore.Security.Accounts.User.Exists(name))
                {
                    User user = Sitecore.Security.Accounts.User.FromName(name, false);
                    if (user != null)
                    {
                        MembershipUser membershipUser = Membership.GetUser(user.Name);

                        response.FullName = user.Profile.FullName;
                        response.EmailAddress = membershipUser != null ? membershipUser.Email : null;
                        response.IsAdministrator = user.IsAdministrator;
                        response.Roles = Roles.GetRolesForUser(user.Name).ToList();
                        if (user.IsAdministrator || Roles.IsUserInRole(RoleNames.BotAnalytics))
                        {
                            response.ApplicationInsights.ApplicationId = Sitecore.Configuration.Settings.GetSetting("sitecore.bot.ai.appId");
                            response.ApplicationInsights.ApiKey = Sitecore.Configuration.Settings.GetSetting("sitecore.bot.ai.apiKey");
                        }
                        response.Success = true;
                    };
                }
                else
                {
                    response.Message = "the user doesn't exist";
                    response.Success = false;
                }

                return new JsonResult<Models.UserManagement.UserProfile>(response, new JsonSerializerSettings(), Encoding.UTF8, this);
            }
        }

        [HttpGet]
        [Route("_bot/api/profile/list")]
        public IHttpActionResult GetUSers([FromUri] string domain = null, [FromUri] string role = null)
        {
            if (!IsAuthenticated)
            {
                return Unauthorized();
            }

            using (new UserSwitcher(Context.Site.Domain.GetFullName(this.ApiUser), false))
            {
                if (!Context.User.IsAdministrator)
                {
                    return Unauthorized();
                }

                var response = new List<UserProfile>();

                if (!string.IsNullOrWhiteSpace(domain) && !string.IsNullOrWhiteSpace(role))
                {
                    role = string.Concat($"{domain}\\{role}");
                }
                else if (string.IsNullOrWhiteSpace(domain) && !string.IsNullOrWhiteSpace(role))
                {
                    role = string.Concat($"{Domain.Current.Name}\\{role}");
                }

                var users = UserManager.GetUsers();
                IEnumerable<User> filteredUsers;

                if (!string.IsNullOrWhiteSpace(role) && Role.Exists(role))
                {
                    filteredUsers = users.Where(u => u.IsInRole(Role.FromName(role)));
                }
                else if (role == $"{Domain.Current.Name}\\administrators" || role == $"{Domain.Current.Name}\\admins")
                {
                    filteredUsers = users.Where(u => u.IsAdministrator);
                }
                else
                {
                    filteredUsers = users.Where(u => u.Domain == Domain.Current);
                }

                if (filteredUsers.Any())
                {
                    response = filteredUsers
                        .Select(u =>
                        {
                            MembershipUser membership = Membership.GetUser(u.Name);
                            
                            var profile = new UserProfile
                            {
                                EmailAddress = u.Profile.Email,
                                FullName = u.Profile.FullName,
                                IsAdministrator = u.IsAdministrator,
                                Roles = u.Roles.Select(r => r.Name).ToList(),
                                LastLogin = membership == null ? (DateTime?)null : membership.LastLoginDate,
                                LastActivity = membership == null ? (DateTime?)null : membership.LastActivityDate,
                                IsOnline = membership == null ? false : membership.IsOnline,
                                UserName = u.Name
                            };

                            return profile;
                        })
                        .ToList();
                }

                return new JsonResult<List<UserProfile>>(response, new JsonSerializerSettings(), Encoding.UTF8, this);
            }
        }

        [HttpPost]
        [Route("_bot/api/profile/create")]
        public IHttpActionResult CreateUser([FromBody] CreateUserRequest create)
        {
            if (!IsAuthenticated)
            {
                return Unauthorized();
            }

            using (new UserSwitcher(Context.Site.Domain.GetFullName(this.ApiUser), false))
            {
                if (!Context.User.IsAdministrator)
                {
                    return Unauthorized();
                }

                var response = new Models.UserManagement.UserProfile();

                if (Sitecore.Security.Accounts.User.Exists(Context.Site.Domain.GetFullName(create.UserName)))
                {
                    response.Message = "the user already exists";
                    response.Success = false;
                }
                else
                {
                    try
                    {
                        string password = "Welkom01";
                        User user = Sitecore.Security.Accounts.User.Create(Context.Site.Domain.GetFullName(create.UserName), password);
                        user.Profile.FullName = create.FullName;
                        user.Profile.Email = create.EmailAddress;
                        user.Profile.IsAdministrator = create.AdministratorRoleForUser;
                        user.Profile.Comment = "Created from bot";
                        user.Profile.Save();

                        Log.Audit(this, "[BOT] Created user: {0}", new string[] { user.Name });

                        if (create.EmailSendWithPasswordToTheUser)
                        {
                            //TODO Send email
                        }

                        response.Success = true;
                    }
                    catch (Exception ex)
                    {
                        response.Success = false;
                        response.Message = ex.Message;
                    }
                }

                return new JsonResult<Models.UserManagement.UserProfile>(response, new JsonSerializerSettings(), Encoding.UTF8, this);
            }
        }

        [HttpPost]
        [Route("_bot/api/profile/enable")]
        public IHttpActionResult EnableUser([FromBody] string username)
        {
            if (!IsAuthenticated)
            {
                return Unauthorized();
            }

            using (new UserSwitcher(Context.Site.Domain.GetFullName(this.ApiUser), false))
            {
                if (!Context.User.IsAdministrator)
                {
                    return Unauthorized();
                }

                var response = new Models.UserManagement.UserProfile();

                string name = Sitecore.Context.Site.Domain.GetFullName(username);
                if (!Sitecore.Security.Accounts.User.Exists(name))
                {
                    response.Message = "the user doesn't exist";
                    response.Success = false;
                }
                else
                {
                    User user = Sitecore.Security.Accounts.User.FromName(name, false);
                    MembershipUser membershipUser = Membership.GetUser(user.Name);

                    if (user == null || membershipUser == null)
                    {
                        response.Message = "the user doesn't exist";
                        response.Success = false;
                    }
                    else if (user.IsAdministrator && user.Name.Equals(ApiUser))
                    {
                        response.Message = "you cannot disable your own account";
                        response.Success = false;
                    }
                    else if (membershipUser.IsApproved)
                    {
                        response.Message = "user isn't disabled";
                        response.Success = false;
                    }
                    else
                    {
                        try
                        {
                            membershipUser.IsApproved = true;
                            Membership.UpdateUser(membershipUser);

                            user.Profile.Comment = "Enabled by bot";
                            user.Profile.Save();

                            Log.Audit(this, "[BOT] Enable user: {0}", new string[] { user.Name });

                            response.Success = membershipUser.IsApproved;
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            response.Message = ex.Message;
                        }
                    }
                }

                return new JsonResult<Models.UserManagement.UserProfile>(response, new JsonSerializerSettings(), Encoding.UTF8, this);
            }
        }

        [HttpPost]
        [Route("_bot/api/profile/disable")]
        public IHttpActionResult DisableUser([FromBody] string username)
        {
            if (!IsAuthenticated)
            {
                return Unauthorized();
            }

            using (new UserSwitcher(Context.Site.Domain.GetFullName(this.ApiUser), false))
            {
                if (!Context.User.IsAdministrator)
                {
                    return Unauthorized();
                }

                var response = new Models.UserManagement.UserProfile();

                string name = Context.Site.Domain.GetFullName(username);
                if (!Sitecore.Security.Accounts.User.Exists(name))
                {
                    response.Message = "the user doesn't exist";
                    response.Success = false;
                }
                else
                {
                    User user = Sitecore.Security.Accounts.User.FromName(name, false);
                    MembershipUser membershipUser = Membership.GetUser(user.Name);

                    if (user == null || membershipUser == null)
                    {
                        response.Message = "the user doesn't exist";
                        response.Success = false;
                    }
                    else if (user.IsAdministrator && user.Name.Equals(ApiUser))
                    {
                        response.Message = "you cannot disable your own account";
                        response.Success = false;
                    }
                    else if (!membershipUser.IsApproved)
                    {
                        response.Message = "user is already disabled";
                        response.Success = false;
                    }
                    else
                    {
                        try
                        {
                            membershipUser.IsApproved = false;
                            Membership.UpdateUser(membershipUser);
                            user.Profile.Comment = "Disabled by bot";
                            user.Profile.Save();

                            Log.Audit(this, "[BOT] Disable user: {0}", new string[] { user.Name });
                            response.Success = !membershipUser.IsApproved;
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            response.Message = ex.Message;
                        }
                    }
                }

                return new JsonResult<Models.UserManagement.UserProfile>(response, new JsonSerializerSettings(), Encoding.UTF8, this);
            }
        }

        [HttpDelete]
        [Route("_bot/api/profile/delete")]
        public IHttpActionResult DeleteUser([FromUri] string username)
        {
            if (!IsAuthenticated)
            {
                return Unauthorized();
            }

            using (new UserSwitcher(Context.Site.Domain.GetFullName(this.ApiUser), false))
            {
                if (!Context.User.IsAdministrator)
                {
                    return Unauthorized();
                }

                var response = new Models.UserManagement.UserProfile();

                string name = Sitecore.Context.Site.Domain.GetFullName(username);
                if (!Sitecore.Security.Accounts.User.Exists(name))
                {
                    response.Message = "the user doesn't exist";
                    response.Success = false;
                }
                else
                {
                    User user = Sitecore.Security.Accounts.User.FromName(Context.Site.Domain.GetFullName(username), false);
                    MembershipUser membershipUser = Membership.GetUser(user.Name);

                    if (user == null || membershipUser == null)
                    {
                        response.Message = "the user doesn't exist";
                        response.Success = false;
                    }
                    else if (user.IsAdministrator && user.Name.Equals(ApiUser))
                    {
                        response.Message = "you cannot delete your own account";
                        response.Success = false;
                    }
                    else
                    {
                        try
                        {
                            Log.Audit(this, "[BOT] Delete user: {0}", new string[] { user.Name });
                            Membership.DeleteUser(user.Name);
                            response.Success = true;
                        }
                        catch (Exception ex)
                        {
                            response.Success = false;
                            response.Message = ex.Message;
                        }
                    }
                }

                return new JsonResult<Models.UserManagement.UserProfile>(response, new JsonSerializerSettings(), Encoding.UTF8, this);
            }
        }
    }
}