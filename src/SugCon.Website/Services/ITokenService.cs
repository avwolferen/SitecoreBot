using System;

namespace SugCon.Website.Services
{
    public interface ITokenService
    {
        string TokenDecoder(string token);
        string TokenEncoder(string token, TimeSpan lifetime);
    }
}