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
            if (tags.Length == 0)
                return;

            var sanitizedTags = tags.ToLower();

            var user = await _context.Users.FindAsync(Context.User.Id);
            if(user == null)
            {
                user = new Models.User() 
                { 
                    UserId = Context.User.Id,
                    SubscribedTags = new List<Models.Tag>()
                };

                _context.Users.Add(user);
            }

            var databaseTags = await _context.Tags.Where(tag => sanitizedTags.Contains(tag.TagName)).ToArrayAsync();
            for (int i = 0; i < databaseTags.Length; i++)
            {
                user.SubscribedTags.Add(databaseTags[i]);
                databaseTags[i].Subscribers.Add(user);
            }

            foreach (var newTagName in sanitizedTags.Except(databaseTags.Select(t => t.TagName)))
            {
                var newTag = new Models.Tag()
                {
                    TagName = newTagName,
                    Subscribers = new List<Models.User>()
                };

                user.SubscribedTags.Add(newTag);
                newTag.Subscribers.Add(user);
            }

            await _context.SaveChangesAsync();
            await ReplyAsync("succesfully subscribed to the tags.");
        }

        [Command("unsubscribe")]
        public async Task Unsubscribe(params string[] tags)
        {
            if (tags.Length == 0)
                return;

            var sanitizedTags = tags.ToLower();

            var user = await _context.Users.FindAsync(Context.User.Id);
            if (user == null)
            {
                user = new Models.User()
                {
                    UserId = Context.User.Id
                };

                _context.Users.Add(user);
            }

            var databaseTags = await _context.Tags.Where(tag => sanitizedTags.Contains(tag.TagName)).ToArrayAsync();
            for (int i = 0; i < databaseTags.Length; i++)
            {
                user.SubscribedTags.Remove(databaseTags[i]);
                databaseTags[i].Subscribers.Remove(user);
            }

            await _context.SaveChangesAsync();
            await ReplyAsync("succesfully unsubscribed from the tags.");
        }

        [Command("blacklist")]
        public async Task Blacklist(params string[] tags)
        {
            if (tags.Length == 0)
                return;

            var sanitizedTags = tags.ToLower();

            var user = await _context.Users.FindAsync(Context.User.Id);
            if (user == null)
            {
                user = new Models.User()
                {
                    UserId = Context.User.Id,
                    BlacklistedTags = new List<Models.Tag>()
                };

                _context.Users.Add(user);
            }

            var databaseTags = await _context.Tags.Where(tag => sanitizedTags.Contains(tag.TagName)).ToArrayAsync();
            for (int i = 0; i < databaseTags.Length; i++)
            {
                user.BlacklistedTags.Add(databaseTags[i]);
                databaseTags[i].Blacklisters.Add(user);
            }

            foreach (var newTagName in sanitizedTags.Except(databaseTags.Select(t => t.TagName)))
            {
                var newTag = new Models.Tag()
                {
                    TagName = newTagName,
                    Subscribers = new List<Models.User>()
                };

                user.BlacklistedTags.Add(newTag);
                newTag.Blacklisters.Add(user);
            }

            await _context.SaveChangesAsync();
            await ReplyAsync("succesfully blacklisted the tags.");
        }
    }
}
