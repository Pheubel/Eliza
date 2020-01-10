using ElizaBot.DatabaseContexts;
using ElizaWebClient.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElizaWebClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public TagsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagMetaData>>> GetTags()
        {
            return await _context.Tags
                .Include(t => t.Subscribers)
                .Include(t => t.Blacklisters)
                .Select(t => new TagMetaData
                {
                    TagName = t.TagName,
                    SubscriberCount = t.Subscribers.Count,
                    BlacklisterCount = t.Blacklisters.Count
                }).ToListAsync();
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TagMetaData>> GetTag(int id)
        {
            var tag = await _context.Tags
                .Where(t => t.Id == id)
                .Include(t => t.Subscribers)
                .Include(t => t.Blacklisters)
                .Select(t => new TagMetaData
                {
                    TagName = t.TagName,
                    SubscriberCount = t.Subscribers.Count,
                    BlacklisterCount = t.Blacklisters.Count
                }).FirstOrDefaultAsync();

            if (tag == null)
            {
                return NotFound();
            }

            return tag;
        }
    }
}
