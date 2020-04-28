using Eliza.Shared;
using System.Threading.Tasks;

namespace Eliza.Client.Services.Core
{
    public interface ITagApi
    {
        Task<TagMetaDataDTO[]> GetTagMetaDataAsync();
        Task<UserTagListDTO> GetUserTaglistAsync();
        Task SubscribeAsync(string[] tags);
        Task UnsubscribeAsync(string[] tags);
        Task BlacklistAsync(string[] tags);
        Task UnblacklistAsync(string[] tags);
    }
}
