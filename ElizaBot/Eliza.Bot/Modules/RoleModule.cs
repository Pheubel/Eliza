using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Eliza.Bot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Eliza.Bot.Modules
{
    public class RoleModule : ModuleBase
    {
        private readonly IRoleService _roleService;

        public RoleModule(IRoleService roleService)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }

        [Command("give role")]
        public async Task GiveRoleAsync(IRole role)
        {
            if (!(Context.User is SocketGuildUser user))
                return;

            await _roleService.GiveRoleToUserAsync(user,role);
        }

        [Command("take role")]
        public async Task TakeRoleAsync(IRole role)
        {
            if (!(Context.User is SocketGuildUser user))
                return;

            await _roleService.TakeRoleFromUserAsync(user,role);
        }
    }
}
