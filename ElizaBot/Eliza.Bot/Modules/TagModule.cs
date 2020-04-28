using Discord;
using Discord.Commands;
using Eliza.Database.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eliza.Bot.Modules
{
    /// <summary> The module responsible for tag related commands.</summary>
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public class TagModule : ModuleBase
    {
        /// <summary> The application database context for interfacing with the database.</summary>
        private readonly TagService _tagService;

        public TagModule(TagService tagService)
        {
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
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
            var user = await _tagService.GetUserWithTaglist(Context.User.Id);

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

            var usersToTag = await _tagService.GetUserToTagById(Context.User.Id, tags);

            // if there are no users to tag, end early
            if (usersToTag.Length == 0)
                return;

            // construct a message containing the users to tag
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < usersToTag.Length; i++)
            {
                var user = await Context.Guild.GetUserAsync(usersToTag[i]);
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

            await _tagService.AddSubscibedTagsToUser(Context.User.Id, tags);
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

            await _tagService.RemoveSubscribedTagsFromUser(Context.User.Id, tags);
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

            await _tagService.AddBlacklistToUser(Context.User.Id, tags);
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

            await _tagService.RemoveBlacklistFromUser(Context.User.Id, tags);
            await ReplyAsync("Succesfully removed the tags from your blacklist.");
        }
    }
}

