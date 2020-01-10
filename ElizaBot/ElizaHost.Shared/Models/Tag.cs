using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ElizaBot.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string TagName { get; set; }

        public List<UserSubcribedTag> Subscribers { get; set; }
        public List<UserBlacklistedTag> Blacklisters { get; set; }
    }
}
