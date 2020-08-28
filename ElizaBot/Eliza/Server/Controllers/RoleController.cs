using Eliza.Bot.Services;
using Eliza.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Eliza.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }

        [HttpGet("guild")]
        public Task<IEnumerable<RoleDTO>> GetDiscordRolesAsync(ulong guildId) =>
            _roleService.GetDiscordRolesAsync(guildId);

        [HttpGet("user")]
        public Task<IEnumerable<RoleDTO>> GetUserDiscordRolesAsync(ulong guildId, ulong userId) =>
            _roleService.GetUserDiscordRolesAsync(guildId, userId);

        [HttpGet("requestable")]
        public Task<ulong[]> GetRequestableRolesAsync(ulong guildId) =>
            _roleService.GetRequestableRoleIdsAsync(guildId);

        [HttpPost("give")]
        public async Task<IActionResult> GiveRoleAsync(ulong guildId, ulong roleId)
        {
            if (!ulong.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return base.Unauthorized();

            var result = await _roleService.GiveRoleToUserAsync(guildId, userId, roleId);

            switch (result)
            {
                case IRoleService.Result.Success:
                    return Ok();
                case IRoleService.Result.GuildNotFound:
                case IRoleService.Result.UserNotFound:
                case IRoleService.Result.RoleNotFound:
                    return NotFound($"result code: {result}");
                default:
                    return Problem(detail: $"result code: {result}", statusCode: 424);
            }
        }

        [HttpPost("take")]
        public async Task<IActionResult> TakeRoleAsync(ulong guildId, ulong roleId)
        {
            if (!ulong.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return base.Unauthorized();

            var result = await _roleService.TakeRoleFromUserAsync(guildId, userId, roleId);

            switch (result)
            {
                case IRoleService.Result.Success:
                    return Ok();
                case IRoleService.Result.GuildNotFound:
                case IRoleService.Result.UserNotFound:
                case IRoleService.Result.RoleNotFound:
                    return NotFound($"result code: {result}");
                default:
                    return BadRequest($"result code: {result}");
            }
        }

        [HttpPost("setrequestable")]
        [Authorize(Eliza.Shared.Constants.IsBotOwner)]
        public async Task<IActionResult> SetRoleRequestableAsync(ulong guildId, ulong roleId)
        {
            var result = await _roleService.SetRoleRequestableAsync(guildId, roleId);

            switch (result)
            {
                case IRoleService.Result.Success:
                    return Ok();
                case IRoleService.Result.GuildNotFound:
                case IRoleService.Result.RoleNotFound:
                    return NotFound($"result code: {result}"); ;
                default:
                    return BadRequest($"result code: {result}");
            }
        }

        [HttpDelete("unsetrequestable")]
        [Authorize(Eliza.Shared.Constants.IsBotOwner)]
        public async Task<IActionResult> UnsetRoleRequestableAsync(ulong roleId)
        {
            var result = await _roleService.UnsetRoleRequestableAsync(roleId);

            switch (result)
            {
                case IRoleService.Result.Success:
                    return Ok();
                case IRoleService.Result.GuildNotFound:
                case IRoleService.Result.RoleNotFound:
                    return NotFound($"result code: {result}"); ;
                default:
                    return BadRequest($"result code: {result}");
            }
        }
    }
}