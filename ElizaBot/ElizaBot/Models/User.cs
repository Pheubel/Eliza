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

        public List<UserSubcribedTag> SubscribedTags { get; set; }
        public List<UserBlacklistedTag> BlacklistedTags { get; set; }
    }
}
