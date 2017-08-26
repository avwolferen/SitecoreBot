using Sitecore.SecurityModel.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SugCon.Website.Services
{
    public class TokenService : ITokenService
    {
        public string TokenEncoder(string token, TimeSpan lifetime)
        {
            token = MachineKeyEncryption.Encode(token + "^" + (DateTime.UtcNow + lifetime).Ticks);

            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(token));
        }

        public string TokenDecoder(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            token = Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(token));

            if (!MachineKeyEncryption.TryDecode(token, out token))
            {
                return null;
            }

            string[] decoded = token.Split('^');

            return new DateTime(long.Parse(decoded[1])) > DateTime.UtcNow
                ? decoded[0]
                : null;
        }
    }
}