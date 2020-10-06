using Eliza.Client.Services.Core;
using Eliza.Shared;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Eliza.Client.Services
{
    public class AuthorizeApi : IAuthorizeApi
    {
        private readonly HttpClient _httpClient;

        public AuthorizeApi(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<UserInfoDTO> GetUserInfo() =>
            await _httpClient.GetFromJsonAsync<UserInfoDTO>("api/elizabot/authentication/userinfo");

        public async Task Login()
        {
            var result = await _httpClient.GetAsync("api/elizabot/authentication/login");
            if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                throw new Exception(await result.Content.ReadAsStringAsync());
            result.EnsureSuccessStatusCode();
        }

        public async Task Logout()
        {
            var result = await _httpClient.PostAsync("api/elizabot/Authorize/Logout", null);
            result.EnsureSuccessStatusCode();
        }
    }
}
