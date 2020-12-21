using System;
using System.Collections.Generic;
using System.Text;

namespace Eliza.Shared
{
    public class GuildWithRolesDTO
    {
        public string GuildName { get; set; }
        public ulong GuildId { get; set; }
        public IEnumerable<RoleDTO> Roles { get; set; }
    }
}
