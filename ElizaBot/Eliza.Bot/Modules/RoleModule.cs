using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Eliza.Bot.Modules
{
    public class RoleModule : ModuleBase
    {
        [Command("give role")]
        public async Task GiveRoleAsync(IRole role)
        {
            if (!(Context.User is SocketGuildUser user))
                return;
            await user.AddRoleAsync(role);
        }

        [Command("take role")]
        public async Task TakeRoleAsync(IRole role)
        {
            if (!(Context.User is SocketGuildUser user))
                return;
            await user.RemoveRoleAsync(role);
        }
    }
}
