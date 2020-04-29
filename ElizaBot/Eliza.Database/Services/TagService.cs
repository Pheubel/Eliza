using Eliza.Database.DatabaseContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Eliza.Database.Services
{
    public class TagService
    {
        private readonly ApplicationContext _context;

        public TagService(ApplicationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Models.User> GetUserWithTaglist(ulong idInvoker)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.SubscribedTags).ThenInclude(st => st.Tag)
                .Include(u => u.BlacklistedTags).ThenInclude(st => st.Tag)
                .FirstOrDefaultAsync(u => u.UserId == idInvoker);
        }

        public async Task<IEnumerable<Eliza.Shared.TagMetaDataDTO>> GetTagMetaData()
        {
            return await _context.Tags
                .AsNoTracking()
                .Include(t => t.Subscribers)
                .Include(t => t.Blacklisters)
                .Select((tag) => new Eliza.Shared.TagMetaDataDTO
                {
                    TagName = tag.TagName,
                    SubscriberCount = tag.Subscribers.Count,
                    BlacklisterCount = tag.Blacklisters.Count
                })
                .ToArrayAsync();
        }

        public async Task<ulong[]> GetUserToTagById(ulong idInvoker, [DisallowNull]string[] tags)
        {
            var sanitizedTags = ArrayToLower(tags);

            // get a collection of users who have subscribed to one of the tags with none of the tags in their blacklist, excluding the invoker
            return await _context.Users
                .AsNoTracking()
                .Where(user => user.UserId != idInvoker && !user.BlacklistedTags.Any(t => sanitizedTags.Contains(t.Tag.TagName)) && user.SubscribedTags.Any(t => sanitizedTags.Contains(t.Tag.TagName)))
                .Select(user => user.UserId)
                .ToArrayAsync();
        }

        public async Task AddSubscibedTagsToUser(ulong idInvoker, [DisallowNull] string[] tags)
        {
            // sanitize the tags by converting them to lower
            var sanitizedTags = ArrayToLower(tags);

            // retrieves the invoker from the database
            var user = await _context.Users
                .Include(u => u.SubscribedTags)
                .Include(u => u.BlacklistedTags)
                .FirstOrDefaultAsync(u => u.UserId == idInvoker);

            if (user == null)
            {
                user = new Models.User()
                {
                    UserId = idInvoker,
                    SubscribedTags = new List<Models.UserSubcribedTag>()
                };

                _context.Users.Add(user);
            }

            // retrieves the tags the invoker wants to subscribe to from the database
            var databaseTags = await _context.Tags.Where(tag => sanitizedTags.Contains(tag.TagName)).ToArrayAsync();

            // removes the blacklisted tags that will be marked as subscribed
            user.BlacklistedTags?.RemoveAll(t => databaseTags.Any(dbt => dbt.Id == t.TagId));

            for (int i = 0; i < databaseTags.Length; i++)
            {
                if (user.SubscribedTags.Any(t => t.TagId == databaseTags[i].Id))
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
        }

        public async Task RemoveSubscribedTagsFromUser(ulong invokerId, [DisallowNull]string[] tags)
        {
            // sanitize the tags by converting them to lower
            var sanitizedTags = ArrayToLower(tags);

            // gets the tags the invoker wishes to unsubscribe from
            var subscribedTags = await _context.UserSubscribedTags
                .Where(ut => ut.UserId == invokerId && sanitizedTags.Contains(ut.Tag.TagName))
                .ToArrayAsync();

            foreach (var subscribedTag in subscribedTags)
            {
                _context.UserSubscribedTags.Remove(subscribedTag);
            }

            if (subscribedTags.Length != 0)
                await _context.SaveChangesAsync();
        }

        public async Task AddBlacklistToUser(ulong invokerId, [DisallowNull]string[] tags)
        {
            // sanitize the tags by converting them to lower
            var sanitizedTags = ArrayToLower(tags);

            // retrieves the invoker from the database
            var user = await _context.Users
                .Include(u => u.SubscribedTags)
                .Include(u => u.BlacklistedTags)
                .FirstOrDefaultAsync(u => u.UserId == invokerId);

            if (user == null)
            {
                user = new Models.User()
                {
                    UserId = invokerId,
                    BlacklistedTags = new List<Models.UserBlacklistedTag>()
                };

                _context.Users.Add(user);
            }

            // retrieves the tags the invoker wants to blacklist from the database
            var databaseTags = await _context.Tags.Where(tag => sanitizedTags.Contains(tag.TagName)).ToArrayAsync();

            // removes the subscribed tags that will be marked as blacklisted
            user.SubscribedTags?.RemoveAll(t => databaseTags.Any(dbt => dbt.Id == t.TagId));

            for (int i = 0; i < databaseTags.Length; i++)
            {
                if (user.BlacklistedTags.Any(t => t.TagId == databaseTags[i].Id))
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
        }

        public async Task RemoveBlacklistFromUser(ulong invokerId, [DisallowNull]string[] tags)
        {
            // sanitize the tags by converting them to lower
            var sanitizedTags = ArrayToLower(tags);

            // gets the tags the invoker wishes to remove from their blacklist
            var blacklistedTags = await _context.UserBlacklistedTags
                .Where(ut => ut.UserId == invokerId && sanitizedTags.Contains(ut.Tag.TagName))
                .ToArrayAsync();

            foreach (var blacklistedTag in blacklistedTags)
            {
                _context.UserBlacklistedTags.Remove(blacklistedTag);
            }

            if (blacklistedTags.Length != 0)
                await _context.SaveChangesAsync();
        }

        private string[] ArrayToLower(string[] array)
        {
            string[] lowerArray = new string[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                lowerArray[i] = array[i].ToLower();
            }

            return lowerArray;
        }
    }
}
