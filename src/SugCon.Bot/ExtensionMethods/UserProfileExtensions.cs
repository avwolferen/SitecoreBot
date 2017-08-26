namespace SugCon.SitecoreBot.ExtensionMethods
{
    using SugCon.SitecoreBot.Models;
    using SugCon.SitecoreBot.Services;
    using System.Threading.Tasks;

    public static class UserProfileExtensions
    {
        public async static Task<bool> IsAuthenticated(this UserProfile profile)
        {
            if (profile == null || string.IsNullOrWhiteSpace(profile.AccessToken))
            {
                return false;
            }

            return await SitecoreAuthenticationAPI.Instance().ValidateAccessToken(profile.AccessToken);
        }
    }
}