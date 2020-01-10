using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
