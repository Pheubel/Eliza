using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Eliza.Bot.Services
{
    public class RoleService : IRoleService
    {
        private readonly DiscordSocketClient _client;

        public RoleService(DiscordSocketClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task GiveRoleToUserAsync(IGuildUser user, IRole role) =>
            user.AddRoleAsync(role);
        

        public async Task<IRoleService.Result> GiveRoleToUserAsync(ulong guildId, ulong userId, ulong roleId)
        {
            var guild = _client.GetGuild(guildId);
            if (guild == null)
                return IRoleService.Result.GuildNotFound;
            var user = guild.GetUser(userId);
            if (user == null)
                return IRoleService.Result.UserNotFound;
            var role = guild.GetRole(roleId);
            if (role == null)
                return IRoleService.Result.RoleNotFound;

            await user.AddRoleAsync(role);
            return IRoleService.Result.Success;
        }

        public Task TakeRoleFromUserAsync(IGuildUser user, IRole role) =>
            user.RemoveRoleAsync(role);

        public async Task<IRoleService.Result> TakeRoleFromUserAsync(ulong guildId, ulong userId, ulong roleId)
        {
            var guild = _client.GetGuild(guildId);
            if (guild == null)
                return IRoleService.Result.GuildNotFound;
            var user = guild.GetUser(userId);
            if (user == null)
                return IRoleService.Result.UserNotFound;
            var role = guild.GetRole(roleId);
            if (role == null)
                return IRoleService.Result.RoleNotFound;

            await user.RemoveRoleAsync(role);
            return IRoleService.Result.Success;
        }
    }
}
