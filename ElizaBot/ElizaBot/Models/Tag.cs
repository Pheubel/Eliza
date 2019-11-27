using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ElizaBot.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string TagName { get; set; }

        [InverseProperty(nameof(User.SubscibedTags))]
        public List<User> Subscribers { get; set; }
        [InverseProperty(nameof(User.BlacklistedTags))]
        public List<User> Blacklisters { get; set; }
    }
}
