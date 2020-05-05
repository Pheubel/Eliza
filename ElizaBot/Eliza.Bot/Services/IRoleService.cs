using Discord;
using System.Threading.Tasks;

namespace Eliza.Bot.Services
{
    public interface IRoleService
    {
        Task<Result> GiveRoleToUserAsync(IGuildUser user, IRole role);
        Task<Result> GiveRoleToUserAsync(ulong guildId, ulong userId, ulong roleId);
        Task<Result> TakeRoleFromUserAsync(IGuildUser user, IRole role);
        Task<Result> TakeRoleFromUserAsync(ulong guildId, ulong userId, ulong roleId);
        Task<Result> SetRoleRequestable(IRole role);
        Task<Result> SetRoleRequestable(ulong guildId, ulong roleId);
        Task<Result> UnsetRoleRequestable(IRole role);
        Task<Result> UnsetRoleRequestable(ulong roleId);

        public enum Result : byte
        {
            Success,
            GuildNotFound,
            UserNotFound,
            RoleNotFound,
            RoleNotAllowed,
            AlreadyHasRole,
            DoesNotHaveRole,
            RoleAlreadyRequestable,
            RoleNotRequestable
        }
    }
}
