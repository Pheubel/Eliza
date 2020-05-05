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

            var result = await _roleService.GiveRoleToUserAsync(user, role);

            switch (result)
            {
                case IRoleService.Result.Success:
                    await ReplyAsync($"You have been given: `{role.Name}`.");
                    break;
                case IRoleService.Result.RoleNotAllowed:
                    await ReplyAsync("You can not request this role.");
                    break;
                default:
                    await ReplyAsync($"Unhandled result encountered: {result}.");
                    break;
            }
        }

        [Command("take role")]
        public async Task TakeRoleAsync(IRole role)
        {
            if (!(Context.User is SocketGuildUser user))
                return;

            var result = await _roleService.TakeRoleFromUserAsync(user, role);

            switch (result)
            {
                case IRoleService.Result.Success:
                    await ReplyAsync($"You no longer have: `{role.Name}`.");
                    break;
                case IRoleService.Result.RoleNotAllowed:
                    await ReplyAsync("You can not request removal of this role.");
                    break;
                default:
                    await ReplyAsync($"Unhandled result encountered: {result}.");
                    break;
            }
        }
        }
    }
}
