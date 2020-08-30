using Eliza.Client.Services.Core;
using Eliza.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Eliza.Client
{
    public class DiscordAuthenticationStateProvider : AuthenticationStateProvider
    {
        private UserInfoDTO _userInfoCache;
        private readonly IAuthorizeApi _authorizeApi;

        public bool HasValidInfoCache => _userInfoCache != null && _userInfoCache.IsAuthenticated;
        public string Username => _userInfoCache?.Username;
        public string AvatarHash => _userInfoCache?.AvatarHash;
        public ulong UserId => _userInfoCache?.UserId ?? default;
        public IReadOnlyDictionary<string, string> Claims => _userInfoCache?.Claims;

        public DiscordAuthenticationStateProvider(IAuthorizeApi authorizeApi)
        {
            _authorizeApi = authorizeApi ?? throw new ArgumentNullException(nameof(authorizeApi));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();

            try
            {
                var userInfo = await GetUserInfoAsync();
                if (userInfo.IsAuthenticated)
                {
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier,_userInfoCache.UserId.ToString()),
                        new Claim(ClaimTypes.Name,_userInfoCache.Username)
                    };
                    claims.AddRange(userInfo.Claims.Select(c => new Claim(c.Key, c.Value)));
                    identity = new ClaimsIdentity(claims, "Server authentication");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request failed: {e}");
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        private async Task<UserInfoDTO> GetUserInfoAsync()
        {
            if (_userInfoCache != null && _userInfoCache.IsAuthenticated)
                return _userInfoCache;

            _userInfoCache = await _authorizeApi.GetUserInfo();
            return _userInfoCache;
        }
    }
}
