using Discord.Commands;
using ElizaBot.DatabaseContexts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElizaBot.CommandModules
{
    public class TagModule : ModuleBase
    {
        private readonly ApplicationContext _context;

        public TagModule(ApplicationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [Command("tag")]
        public async Task Tag(params string[] tags)
        {

        }

        [Command("subscribe")]
        public async Task Subscribe(params string[] tags)
        {

        }

        [Command("unsubscribe")]
        public async Task Unsubscribe(params string[] tags)
        {

        }
    }
}
