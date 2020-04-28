using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Eliza.Database.Services;
using Eliza.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eliza.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly TagService _tagService;

        public TagsController(TagService tagService)
        {
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagMetaDataDTO>>> GetTags()
        {
            return Ok(await _tagService.GetTagMetaData());
        }

        [HttpGet("userlist")]
        [Authorize]
        public async Task<ActionResult<UserTagListDTO>> GetTagsForUser()
        {
            var claims = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (!ulong.TryParse(claims?.Value, out ulong userId)){
                return Ok(new UserTagListDTO
                {
                    SubscribedTags = Array.Empty<string>(),
                    BlacklistedTags = Array.Empty<string>()
                });
            }

            var user = await _tagService.GetUserWithTaglist(userId);

            if (user == null)
            {
                return Ok(new UserTagListDTO
                {
                    SubscribedTags = Array.Empty<string>(),
                    BlacklistedTags = Array.Empty<string>()
                });
            }
            return Ok(new UserTagListDTO
            {
                SubscribedTags = user.SubscribedTags.Select((subscriptions) => subscriptions.Tag.TagName),
                BlacklistedTags = user.BlacklistedTags.Select((blacklist) => blacklist.Tag.TagName)
            });
        }

        [Authorize]
        [HttpPost("subscribe")]
        public async Task<IActionResult> SubscribeToTags(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return BadRequest("Action requires at least one tag.");
            if (tags.Any(t => t.Contains(' ')))
                return BadRequest("Malformed data, cannot accept tag with space character.");

            var userId = ulong.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            await _tagService.AddSubscibedTagsToUser(userId, tags);
            return Ok();
        }

        [Authorize]
        [HttpPost("unsubscribe")]
        public async Task<IActionResult> UnsubscribeToTags(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return BadRequest("Action requires at least one tag.");
            if (tags.Any(t => t.Contains(' ')))
                return BadRequest("Malformed data, cannot accept tag with space character.");

            var userId = ulong.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            await _tagService.RemoveSubscribedTagsFromUser(userId, tags);
            return Ok();
        }

        [Authorize]
        [HttpPost("blacklist")]
        public async Task<IActionResult> BlacklistTags(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return BadRequest("Action requires at least one tag.");
            if (tags.Any(t => t.Contains(' ')))
                return BadRequest("Malformed data, cannot accept tag with space character.");

            var userId = ulong.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            await _tagService.AddBlacklistToUser(userId, tags);
            return Ok();
        }

        [Authorize]
        [HttpPost("unblacklist")]
        public async Task<IActionResult> UnblacklistTags(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return BadRequest("Action requires at least one tag.");
            if (tags.Any(t => t.Contains(' ')))
                return BadRequest("Malformed data, cannot accept tag with space character.");

            var userId = ulong.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            await _tagService.RemoveSubscribedTagsFromUser(userId, tags);
            return Ok();
        }
    }
}
