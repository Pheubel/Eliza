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

        [Command("tag")]
        public async Task Tag(params string[] tags)
        {
            if (tags.Length == 0)
                return;

            var sanitizedTags = tags.ToLower();

            var usersToTag = await _context.Users
                .AsNoTracking()
                .Where(user => !user.BlacklistedTags.Any(t => sanitizedTags.Contains(t.Tag.TagName)) && user.SubscribedTags.All(t => sanitizedTags.Contains(t.Tag.TagName)))
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
                var subscription = new Models.UserSubcribedTag()
                {
                    User = user,
                    Tag = databaseTags[i]
                };

                user.SubscribedTags.Add(subscription);
                _context.UserSubscribedTags.Add(subscription);
                databaseTags[i].Subscribers.Add(subscription);
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

                user.SubscribedTags.Add(subscription);
                _context.UserSubscribedTags.Add(subscription);
                newTag.Subscribers.Add(subscription);
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
                subscribedTag.User.SubscribedTags.Remove(subscribedTag);
                subscribedTag.Tag.Subscribers.Remove(subscribedTag);
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

            var user = await _context.Users.FindAsync(Context.User.Id);
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
                var blacklistedTag = new Models.UserBlacklistedTag()
                {
                    User = user,
                    Tag = databaseTags[i]
                };

                user.BlacklistedTags.Add(blacklistedTag);
                _context.UserBlacklistedTags.Add(blacklistedTag);
                databaseTags[i].Blacklisters.Add(blacklistedTag);
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

                user.BlacklistedTags.Add(blacklistedTag);
                _context.UserBlacklistedTags.Add(blacklistedTag);
                newTag.Blacklisters.Add(blacklistedTag);
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
                blacklistedTag.User.BlacklistedTags.Remove(blacklistedTag);
                blacklistedTag.Tag.Blacklisters.Remove(blacklistedTag);
                _context.UserBlacklistedTags.Remove(blacklistedTag);
            }

            if (blacklistedTags.Length != 0)
                await _context.SaveChangesAsync();
            await ReplyAsync("succesfully removed the tags from your blacklist.");
        }
    }
}
