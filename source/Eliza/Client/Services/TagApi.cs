using Eliza.Client.Services.Core;
using Eliza.Shared;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Eliza.Client.Services
{
    public class TagApi : ITagApi
    {
        private readonly HttpClient _client;

        public TagApi(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<TagMetaDataDTO[]> GetTagMetaDataAsync() =>
            await _client.GetFromJsonAsync<TagMetaDataDTO[]>("api/elizabot/tags");

        public async Task<UserTagListDTO> GetUserTaglistAsync() =>
            await _client.GetFromJsonAsync<UserTagListDTO>("api/elizabot/tags/userlist");

        public async Task SubscribeAsync(string[] tags) =>
            await _client.PostAsJsonAsync("api/elizabot/tags/subscribe", tags);

        public async Task UnsubscribeAsync(string[] tags) =>
            await _client.PostAsJsonAsync("api/elizabot/tags/unsubscribe", tags);

        public async Task BlacklistAsync(string[] tags) =>
            await _client.PostAsJsonAsync("api/elizabot/tags/blacklist", tags);

        public async Task UnblacklistAsync(string[] tags) =>
            await _client.PostAsJsonAsync("api/elizabot/tags/unblacklist", tags);

    }
}
