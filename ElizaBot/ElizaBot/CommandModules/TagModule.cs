using Discord.Commands;
using ElizaBot.DatabaseContexts;
using ElizaBot.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (tags.Length == 0)
                return;

            var sanitizedTags = tags.ToLower();

            var usersToTag = await _context.Users
                .AsNoTracking()
                .Where(user => !user.BlacklistedTags.Any(t => sanitizedTags.Contains(t.TagName)) && user.SubscribedTags.All(t => sanitizedTags.Contains(t.TagName)))
                .ToArrayAsync();

            if (usersToTag.Length == 0)
                return;

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < usersToTag.Length; i++)
            {
                var user = await Context.Guild.GetUserAsync(usersToTag[i].UserId);
                builder.Append(user.Mention);
            }

            await ReplyAsync(builder.ToString());
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
