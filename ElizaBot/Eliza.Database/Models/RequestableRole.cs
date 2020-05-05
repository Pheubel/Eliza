using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Eliza.Database.Models
{
    public class RequestableRole
    {
        [Key]
        public ulong RoleId { get; set; }
    }
}
