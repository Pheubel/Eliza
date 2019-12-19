using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ElizaBot.Models
{
    public class UserSubcribedTag
    {
        public ulong UserId { get; set; }
        public int TagId { get; set; }

        public User User { get; set; }
        public Tag Tag { get; set; }
    }
}
