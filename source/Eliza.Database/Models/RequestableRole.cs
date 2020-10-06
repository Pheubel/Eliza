using System.ComponentModel.DataAnnotations;

namespace Eliza.Database.Models
{
    public class RequestableRole
    {
        [Key]
        public ulong RoleId { get; set; }
        [MaxLength(32), Required]
        public string RoleName { get; set; }
        public ulong GuildId { get; set; }
    }
}
