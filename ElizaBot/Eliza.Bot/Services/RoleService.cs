using Discord;
using Discord.WebSocket;
using Eliza.Database.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliza.Bot.Services
{
    public class RoleService : IRoleService
    {
        private readonly DiscordSocketClient _client;
        private readonly RequestableRoleManager _requestableRoleManager;

        public RoleService(DiscordSocketClient client, RequestableRoleManager roleManager)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _requestableRoleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<IRoleService.Result> GiveRoleToUserAsync(IGuildUser user, IRole role)
        {
            if (!await _requestableRoleManager.IsRoleRequestableAsync(role.Id))
                return IRoleService.Result.RoleNotAllowed;

            if (user.RoleIds.Contains(role.Id))
                return IRoleService.Result.AlreadyHasRole;

            await user.AddRoleAsync(role);
            return IRoleService.Result.Success;
        }

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

            if (!await _requestableRoleManager.IsRoleRequestableAsync(roleId))
                return IRoleService.Result.RoleNotAllowed;

            if (user.Roles.Any(r => r.Id == roleId))
                return IRoleService.Result.AlreadyHasRole;

            await user.AddRoleAsync(role);
            return IRoleService.Result.Success;
        }

        public async Task<IRoleService.Result> TakeRoleFromUserAsync(IGuildUser user, IRole role)
        {
            if (!await _requestableRoleManager.IsRoleRequestableAsync(role.Id))
                return IRoleService.Result.RoleNotAllowed;

            if (!user.RoleIds.Contains(role.Id))
                return IRoleService.Result.DoesNotHaveRole;

            await user.RemoveRoleAsync(role);
            return IRoleService.Result.Success;
        }

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

            if (!await _requestableRoleManager.IsRoleRequestableAsync(roleId))
                return IRoleService.Result.RoleNotAllowed;

            if (!user.Roles.Any(r => r.Id == roleId))
                return IRoleService.Result.DoesNotHaveRole;

            await user.RemoveRoleAsync(role);
            return IRoleService.Result.Success;
        }
    }
}
