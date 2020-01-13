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
    /// <summary> The module responsible for tag related commands.</summary>
    [RequireBotPermission(Discord.ChannelPermission.SendMessages)]
    public class TagModule : ModuleBase
    {
        /// <summary> The application database context for interfacing with the database.</summary>
        private readonly ApplicationContext _context;

        public TagModule(ApplicationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary> Sends the invoker a complete list of their tag subscriptions and blacklist.</summary>
        /// <example>
        ///     >>list tags
        /// </example>
        [Command("list tags")]
        [Summary("Sends the invoker a complete list of their tag subscriptions and blacklist.")]
        public async Task ListTags()
        {
            // fetch the user from the database alongside his/her subscriptions and blacklist.
            var user = await _context.Users
                .Include(u => u.SubscribedTags).ThenInclude(st => st.Tag)
                .Include(u => u.BlacklistedTags).ThenInclude(st => st.Tag)
                .FirstOrDefaultAsync(u => u.UserId == Context.User.Id);

            // make sure a user has been found and that the found user has tags in their lists
            if (user == null || (user.SubscribedTags.Count == 0 && user.BlacklistedTags.Count == 0))
            {
                await ReplyAsync("You do not have any subscriptions or blacklisted tags.");
                return;
            }

            // fetch the DM channel of the target user.
            var userPrivateChannel = await Context.User.GetOrCreateDMChannelAsync();
            
            
            StringBuilder sb = new StringBuilder();

            // construct the embed for subscribed tags.
            if (user.SubscribedTags.Count > 0)
            {
                var embedBuilder = new EmbedBuilder
                {
                    Title = "Subscribed tags",
                    Color = Color.Green
                };

                // loop through each tag in alphabetical order
                foreach (var tagSubscription in user.SubscribedTags.OrderBy(t => t.Tag.TagName))
                {
                    sb.Append(tagSubscription.Tag.TagName + ", ");

                    if (sb.Length > 1800)
                    {
                        sb.Remove(sb.Length - 2, 2);
                        embedBuilder.Description = sb.ToString();

                        await userPrivateChannel.SendMessageAsync(embed: embedBuilder.Build());

                        sb.Clear();
                        embedBuilder.Title = string.Empty;
                    }
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 2, 2);
                    embedBuilder.Description = sb.ToString();

                    await userPrivateChannel.SendMessageAsync(embed: embedBuilder.Build());

                    sb.Clear();
                }
            }

            // construct the embed for blacklisted tags.
            if (user.BlacklistedTags.Count > 0)
            {
                var embedBuilder = new EmbedBuilder
                {
                    Title = "Blacklisted tags",
                    Color = Color.Red
                };

                // loop through each tag in alphabetical order
                foreach (var tagBlacklisting in user.BlacklistedTags.OrderBy(t => t.Tag.TagName))
                {
                    sb.Append(tagBlacklisting.Tag.TagName + ", ");

                    if (sb.Length > 1800)
                    {
                        sb.Remove(sb.Length - 2, 2);
                        embedBuilder.Description = sb.ToString();

                        await userPrivateChannel.SendMessageAsync(embed: embedBuilder.Build());

                        sb.Clear();
                        embedBuilder.Title = string.Empty;
                    }
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 2, 2);
                    embedBuilder.Description = sb.ToString();

                    await userPrivateChannel.SendMessageAsync(embed: embedBuilder.Build());

                    sb.Clear();
                }
            }

            // send a message to redirect the target user to their DM's
            if (Context.Channel.Id != userPrivateChannel.Id)
                await ReplyAsync("A list subscriptions and blacklists has been sent to your DM's.");
        }

        /// <summary> Tags users who have been subscribed to the image tags provided in the command, excluding users with one of the tags in their blacklist and the invoker.</summary>
        /// <param name="tags"> The image tags related to the image(s) posted.</param>
        /// <example>
        ///     >>tag swimsuit
        ///     >>tag nintendo samus_aran
        /// </example>
        [Command("tag")]
        [Summary("Tags users who have been subscribed to the image tags provided in the command, excluding users with one of the tags in their blacklist and the invoker.")]
        public async Task Tag(params string[] tags)
        {
            // make sure that there are tags
            if (tags.Length == 0)
                return;

            // sanitize the tags by converting them to lower
            var sanitizedTags = tags.ToLower();

            // get a collection of users who have subscribed to one of the tags with none of the tags in their blacklist, excluding the invoker
            var usersToTag = await _context.Users
                .AsNoTracking()
                .Where(user => user.UserId != Context.User.Id && !user.BlacklistedTags.Any(t => sanitizedTags.Contains(t.Tag.TagName)) && user.SubscribedTags.Any(t => sanitizedTags.Contains(t.Tag.TagName)))
                .ToArrayAsync();

            // if there are no users to tag, end early
            if (usersToTag.Length == 0)
                return;

            // construct a message containing the users to tag
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < usersToTag.Length; i++)
            {
                var user = await Context.Guild.GetUserAsync(usersToTag[i].UserId);
                builder.Append(user?.Mention ?? string.Empty);
            }

            await ReplyAsync(builder.ToString());
        }

        /// <summary> Subscribes the invoker to the image tag(s) to notify the invoker when an image with the tag(s) gets posted.</summary>
        /// <param name="tags"> The image tags the invoker subscribes to.</param>
        /// <example> 
        ///     >>subscribe large_breasts
        ///     >>subscribe futanari
        /// </example>
        [Command("subscribe")]
        [Summary("Subscribes the invoker to the image tag(s) to notify the invoker when an image with the tag(s) gets posted.")]
        public async Task Subscribe(params string[] tags)
        {
            // make sure that there are tags
            if (tags.Length == 0)
                return;

            // sanitize the tags by converting them to lower
            var sanitizedTags = tags.ToLower();

            // retrieves the invoker from the database
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

            // retrieves the tags the invoker wants to subscribe to from the database
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

            // creates the tags that are missing in the database
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
            await ReplyAsync("Succesfully subscribed to the tags.");
        }

        /// <summary> Unsubscribes the invoker from the tag(s) to not get notified by images with the tag(s).</summary>
        /// <param name="tags"> The image tags the invoker unsubscribes from.</param>
        /// <example>
        ///     >>unsubscribe large_breasts
        ///     >>unsubscribe nintendo samus_aran
        /// </example>
        [Command("unsubscribe")]
        [Summary("Unsubscribes the invoker from the tag(s) to not get notified by images with the tag(s).")]
        public async Task Unsubscribe(params string[] tags)
        {
            // make sure that there are tags
            if (tags.Length == 0)
                return;

            // sanitize the tags by converting them to lower
            var sanitizedTags = tags.ToLower();

            // gets the tags the invoker wishes to unsubscribe from
            var subscribedTags = await _context.UserSubscribedTags
                .Where(ut => ut.UserId == Context.User.Id && sanitizedTags.Contains(ut.Tag.TagName))
                .ToArrayAsync();

            foreach (var subscribedTag in subscribedTags)
            {
                _context.UserSubscribedTags.Remove(subscribedTag);
            }

            if (subscribedTags.Length != 0)
                await _context.SaveChangesAsync();
            await ReplyAsync("Succesfully unsubscribed from the tags.");
        }

        /// <summary> Adds the tag(s) to the invoker's blacklist.</summary>
        /// <param name="tags"> The image tags the invoker wants to blacklist.</param>
        /// <example>
        ///     >>blacklist furry
        ///     >>blacklist scat ugly_bastard
        /// </example>
        [Command("blacklist")]
        [Summary("Adds the tag(s) to the invoker's blacklist.")]
        public async Task Blacklist(params string[] tags)
        {
            // make sure that there are tags
            if (tags.Length == 0)
                return;

            // sanitize the tags by converting them to lower
            var sanitizedTags = tags.ToLower();

            // retrieves the invoker from the database
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

            // retrieves the tags the invoker wants to blacklist from the database
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

            // creates the tags that are missing in the database
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
            await ReplyAsync("Succesfully blacklisted the tags.");
        }

        /// <summary>Removes the tag(s) from the invoker's blacklist.</summary>
        /// <param name="tags"> The image tags the invoker wants to remove from their blacklist.</param>
        /// <example>
        ///     >>unblacklist glasses
        ///     >>unblacklist stomache_deformation lactation
        /// </example>
        [Command("unblacklist")]
        [Summary("Removes the tag(s) from the invoker's blacklist.")]
        public async Task Unblacklist(params string[] tags)
        {
            // make sure that there are tags
            if (tags.Length == 0)
                return;

            // sanitize the tags by converting them to lower
            var sanitizedTags = tags.ToLower();

            // gets the tags the invoker wishes to remove from their blacklist
            var blacklistedTags = await _context.UserBlacklistedTags
                .Where(ut => ut.UserId == Context.User.Id && sanitizedTags.Contains(ut.Tag.TagName))
                .ToArrayAsync();

            foreach (var blacklistedTag in blacklistedTags)
            {
                _context.UserBlacklistedTags.Remove(blacklistedTag);
            }

            if (blacklistedTags.Length != 0)
                await _context.SaveChangesAsync();
            await ReplyAsync("Succesfully removed the tags from your blacklist.");
        }
    }
}
