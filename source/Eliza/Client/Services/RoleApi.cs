using Eliza.Client.Services.Core;
using Eliza.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eliza.Client.Services
{
    public class RoleApi : IRoleApi
    {
        private readonly HttpClient _client;

        public RoleApi(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task<IEnumerable<RoleDTO>> GetDiscordRolesAsync(ulong guildId) =>
            _client.GetFromJsonAsync<IEnumerable<RoleDTO>>("");

        public Task<IEnumerable<ulong>> GetRequestableRoleIdsAsync(ulong guildId) =>
            _client.GetFromJsonAsync<IEnumerable<ulong>>("");

        public Task<IEnumerable<RoleDTO>> GetUserDiscordRolesAsync(ulong guildId, ulong userId) =>
            _client.GetFromJsonAsync<IEnumerable<RoleDTO>>("");

        public async Task GiveRoleAsync(ulong guildId, ulong roleId)
        {
            var mesage = await _client.PostAsJsonAsync("", new { guildId, roleId });
            mesage.EnsureSuccessStatusCode();
        }

        public async Task SetRequestableAsync(ulong guildId, ulong roleId)
        {
            var mesage = await _client.PostAsJsonAsync("", new { guildId, roleId });
            mesage.EnsureSuccessStatusCode();
        }

        public async Task TakeRoleAsync(ulong guildId, ulong roleId)
        {
            var mesage = await _client.PostAsJsonAsync("", new { guildId, roleId });
            mesage.EnsureSuccessStatusCode();
        }

        public async Task UnsetRequestableAsync(ulong guildId, ulong roleId)
        {
            using StringContent content = new StringContent(JsonSerializer.Serialize(new { guildId, roleId }));

            using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Delete, "");
            message.Content = content;

            var response = await _client.SendAsync(message);
            response.EnsureSuccessStatusCode();
        }
    }
}
