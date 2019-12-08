using Discord;
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
    [RequireBotPermission(Discord.ChannelPermission.SendMessages)]
    public class TagModule : ModuleBase
    {
        private readonly ApplicationContext _context;

        public TagModule(ApplicationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [Command("list tags")]
        public async Task ListTags()
        {
            var user = await _context.Users
                .Include(u => u.SubscribedTags).ThenInclude(st => st.Tag)
                .Include(u => u.BlacklistedTags).ThenInclude(st => st.Tag)
                .FirstOrDefaultAsync(u => u.UserId == Context.User.Id);

            if (user == null || (user.SubscribedTags.Count == 0 && user.BlacklistedTags.Count == 0))
            {
                await ReplyAsync("You do not have any subscriptions or blacklisted tags.");
                return;
            }

            var userPrivateChannel = await Context.User.GetOrCreateDMChannelAsync();
            StringBuilder sb = new StringBuilder();

            if (user.SubscribedTags.Count > 0)
            {
                foreach (var tagSubscription in user.SubscribedTags)
                {
                    sb.Append(tagSubscription.Tag.TagName + ", ");
                }
                sb.Remove(sb.Length - 2, 2);

                var embedBuilder = new EmbedBuilder
                {
                    Title = "Subscribed tags",
                    Description = sb.ToString(),
                    Color = Color.Green
                };

                await userPrivateChannel.SendMessageAsync(embed: embedBuilder.Build());

                sb.Clear();
            }

            if (user.BlacklistedTags.Count > 0)
            {
                foreach (var tagBlacklisting in user.BlacklistedTags)
                {
                    sb.Append(tagBlacklisting.Tag.TagName + ", ");
                }
                sb.Remove(sb.Length - 2, 2);

                var embedBuilder = new EmbedBuilder
                {
                    Title = "Blacklisted tags",
                    Description = sb.ToString(),
                    Color = Color.Red
                };

                await userPrivateChannel.SendMessageAsync(embed: embedBuilder.Build());

                sb.Clear();
            }

            if (Context.Channel.Id != userPrivateChannel.Id)
                await ReplyAsync("A list subscriptions and blacklists has been sent to your Dm's.");
        }


        [Command("tag")]
        public async Task Tag(params string[] tags)
        {
            if (tags.Length == 0)
                return;

            var sanitizedTags = tags.ToLower();

            var usersToTag = await _context.Users
                .AsNoTracking()
                .Where(user => user.UserId != Context.User.Id && !user.BlacklistedTags.Any(t => sanitizedTags.Contains(t.Tag.TagName)) && user.SubscribedTags.Any(t => sanitizedTags.Contains(t.Tag.TagName)))
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

            var user = await _context.Users.Include(u => u.SubscribedTags).FirstOrDefaultAsync(u => u.UserId == Context.User.Id);
            if (user == null)
            {
                user = new Models.User()
                {
                    UserId = Context.User.Id,
                    SubscribedTags = new List<Models.UserSubcribedTag>()
                };

                _context.Users.Add(user);
            }

            var databaseTags = await _context.Tags.Where(tag => sanitizedTags.Contains(tag.TagName)).ToArrayAsync();
            for (int i = 0; i < databaseTags.Length; i++)
            {
                if (user.SubscribedTags.Any(t => t.Tag.TagName == databaseTags[i].TagName))
                    continue;

                var subscription = new Models.UserSubcribedTag()
                {
                    User = user,
                    Tag = databaseTags[i]
                };

                _context.UserSubscribedTags.Add(subscription);
            }

            foreach (var newTagName in sanitizedTags.Except(databaseTags.Select(t => t.TagName)))
            {
                var newTag = new Models.Tag()
                {
                    TagName = newTagName,
                    Subscribers = new List<Models.UserSubcribedTag>()
                };

                var subscription = new Models.UserSubcribedTag()
                {
                    User = user,
                    Tag = newTag
                };


                _context.UserSubscribedTags.Add(subscription);
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

            var subscribedTags = await _context.UserSubscribedTags.Where(ut => ut.UserId == Context.User.Id && sanitizedTags.Contains(ut.Tag.TagName)).ToArrayAsync();

            foreach (var subscribedTag in subscribedTags)
            {
                _context.UserSubscribedTags.Remove(subscribedTag);
            }

            if (subscribedTags.Length != 0)
                await _context.SaveChangesAsync();
            await ReplyAsync("succesfully unsubscribed from the tags.");
        }

        [Command("blacklist")]
        public async Task Blacklist(params string[] tags)
        {
            if (tags.Length == 0)
                return;

            var sanitizedTags = tags.ToLower();

            var user = await _context.Users.Include(u => u.BlacklistedTags).FirstOrDefaultAsync(u => u.UserId == Context.User.Id);
            if (user == null)
            {
                user = new Models.User()
                {
                    UserId = Context.User.Id,
                    BlacklistedTags = new List<Models.UserBlacklistedTag>()
                };

                _context.Users.Add(user);
            }

            var databaseTags = await _context.Tags.Where(tag => sanitizedTags.Contains(tag.TagName)).ToArrayAsync();
            for (int i = 0; i < databaseTags.Length; i++)
            {
                if (user.BlacklistedTags.Any(t => t.Tag.TagName == databaseTags[i].TagName))
                    continue;

                var blacklistedTag = new Models.UserBlacklistedTag()
                {
                    User = user,
                    Tag = databaseTags[i]
                };

                _context.UserBlacklistedTags.Add(blacklistedTag);
            }

            foreach (var newTagName in sanitizedTags.Except(databaseTags.Select(t => t.TagName)))
            {
                var newTag = new Models.Tag()
                {
                    TagName = newTagName,
                    Blacklisters = new List<Models.UserBlacklistedTag>()
                };

                var blacklistedTag = new Models.UserBlacklistedTag()
                {
                    User = user,
                    Tag = newTag
                };

                _context.UserBlacklistedTags.Add(blacklistedTag);
            }

            await _context.SaveChangesAsync();
            await ReplyAsync("succesfully blacklisted the tags.");
        }


        [Command("unblacklist")]
        public async Task Unblacklist(params string[] tags)
        {
            if (tags.Length == 0)
                return;

            var sanitizedTags = tags.ToLower();

            var blacklistedTags = await _context.UserBlacklistedTags.Where(ut => ut.UserId == Context.User.Id && sanitizedTags.Contains(ut.Tag.TagName)).ToArrayAsync();

            foreach (var blacklistedTag in blacklistedTags)
            {
                _context.UserBlacklistedTags.Remove(blacklistedTag);
            }

            if (blacklistedTags.Length != 0)
                await _context.SaveChangesAsync();
            await ReplyAsync("succesfully removed the tags from your blacklist.");
        }
    }
}
