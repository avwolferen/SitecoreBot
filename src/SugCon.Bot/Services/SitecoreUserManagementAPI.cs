namespace SugCon.SitecoreBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SugCon.Models.UserManagement;

    public class SitecoreUserManagementAPI : BaseAPI
    {
        public SitecoreUserManagementAPI() : base()
        {
        }

        public SitecoreUserManagementAPI(string access_token) : base(access_token)
        {
        }

        public static SitecoreUserManagementAPI Instance(string access_token)
        {
            return new SitecoreUserManagementAPI(access_token);
        }

        public static SitecoreUserManagementAPI Instance()
        {
            return new SitecoreUserManagementAPI();
        }

        public async Task<List<UserProfile>> GetUsers(string domain, string role)
        {
            var tuples = new List<Tuple<string, string>>();
            if (!string.IsNullOrWhiteSpace(domain))
            {
                tuples.Add(Tuple.Create("domain", domain));
            }
            if (!string.IsNullOrWhiteSpace(role))
            {
                tuples.Add(Tuple.Create("role", role));
            }

            var uri = GetUri($"{ SitecoreUrl}/api/profile/list", tuples.ToArray());

            return await GetRequest<List<UserProfile>>(uri);
        }

        public async Task<UserProfile> GetProfile(string username = null)
        {
            var tuples = new List<Tuple<string, string>>();
            if (!string.IsNullOrWhiteSpace(username))
            {
                tuples.Add(Tuple.Create("username", username));
            }

            var uri = GetUri($"{ SitecoreUrl}/api/profile/get", tuples.ToArray());

            return await GetRequest<UserProfile>(uri);
        }

        public async Task<bool> UserExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            var uri = GetUri($"{ SitecoreUrl}/api/profile/exists", Tuple.Create("username", username));

            return await GetRequest<bool>(uri, $"{SitecoreAppId}|{SitecoreAppSecret}");
        }

        public async Task<UserProfile> EnableUser(string username)
        {
            var uri = GetUri($"{ SitecoreUrl}/api/profile/enable", Tuple.Create("username", username));

            return await PostRequest<string, UserProfile>(uri, username);
        }

        public async Task<UserProfile> DisableUser(string username)
        {
            var uri = GetUri($"{ SitecoreUrl}/api/profile/disable");

            return await PostRequest<string, UserProfile>(uri, username);
        }

        public async Task<UserProfile> CreateUser(Models.Forms.CreateUser request)
        {
            var uri = GetUri($"{ SitecoreUrl}/api/profile/create");
            var response = await PostRequest<Models.Forms.CreateUser, UserProfile>(uri, request);

            return response;
        }

        public async Task<UserProfile> DeleteUser(string username)
        {
            var uri = GetUri($"{ SitecoreUrl}/api/profile/delete", Tuple.Create("username", username));
            var response = await DeleteRequest<UserProfile>(uri);

            return response;
        }
    }
}