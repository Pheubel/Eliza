using Discord;
using System.Threading.Tasks;

namespace Eliza.Bot.Services
{
    public interface IRoleService
    {
        Task GiveRoleToUserAsync(IGuildUser user, IRole role);
        Task GiveRoleToUserAsync(ulong userId, ulong roleId);
        Task TakeRoleFromUserAsync(IGuildUser user, IRole role);
        Task TakeRoleFromUserAsync(ulong userId, ulong roleId);
    }
}
