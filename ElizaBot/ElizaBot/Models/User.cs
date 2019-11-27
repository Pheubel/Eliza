using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ElizaBot.Models
{
    public class User
    {
        [Key]
        public ulong UserId { get; set; }

        [InverseProperty(nameof(Tag.Subscribers))]
        public List<Tag> SubscibedTags { get; set; }
        [InverseProperty(nameof(Tag.Blacklisters))]
        public List<Tag> BlacklistedTags { get; set; }
    }
}
