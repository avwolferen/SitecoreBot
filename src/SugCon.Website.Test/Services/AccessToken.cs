using System;
using NUnit.Framework;
using System.Threading;
using SugCon.Website.Services;

namespace SugCon.Website.Test.Services
{
    public class AccessToken
    {
        [TestCase("abcdefghijklmnopqrstuvwxyz", 2, 0, true)]
        [TestCase("abcdefghijklmnopqrstuvwxyz", 2, 5, false)]
        [TestCase("abcdefghijklmnopqrstuvwxyz", 5, 0, true)]
        [TestCase("abcdefghijklmnopqrstuvwxyz", 10, 0, true)]
        public void TokenEncoder(string input, int seconds, int sleep, bool succes)
        {
            ITokenService tokenService = new TokenService();

            string encoded = tokenService.TokenEncoder(input, new TimeSpan(0, 0, seconds));

            Thread.Sleep(sleep * 1000);

            string decoded = tokenService.TokenDecoder(encoded);

            if (succes)
            {
                Assert.AreEqual(input, decoded);
            }
            else
            {
                Assert.AreNotEqual(input, decoded);
            }
        }
    }
}
