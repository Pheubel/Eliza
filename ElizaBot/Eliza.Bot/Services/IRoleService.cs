using Discord;
using System.Threading.Tasks;

namespace Eliza.Bot.Services
{
    public interface IRoleService
    {
        Task GiveRoleToUserAsync(IGuildUser user, IRole role);
        Task<Result> GiveRoleToUserAsync(ulong guildId, ulong userId, ulong roleId);
        Task TakeRoleFromUserAsync(IGuildUser user, IRole role);
        Task<Result> TakeRoleFromUserAsync(ulong guildId, ulong userId, ulong roleId);

        public enum Result : byte
        {
            Unknown,
            Success,
            GuildNotFound,
            UserNotFound,
            RoleNotFound,
            RoleNotAllowed
        }
    }
}
